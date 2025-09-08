using CryptoMonitor.Infrastructure.Data;
using CryptoMonitor.Core.Interfaces;
using CryptoMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using System.Diagnostics;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.Uris;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/cryptomonitor-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddDbContext<CryptoDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configurar HttpClient para CoinGecko
    builder.Services.AddHttpClient<ICryptoPriceService, CryptoPriceService>(client =>
    {
        client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
        client.DefaultRequestHeaders.Add("User-Agent", "CryptoMonitor/1.0");
    });

    // Registrar outros serviços
    builder.Services.AddScoped<ICryptoPriceService, CryptoPriceService>();

    // Configurar Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<DbContextHealthCheck<CryptoDbContext>>("database")
        .AddUrlGroup(
            new Uri("https://api.coingecko.com/api/v3/ping"),
            name: "coingecko",
            failureStatus: HealthStatus.Degraded,
            timeout: TimeSpan.FromSeconds(5)
        )
        .AddCheck<MemoryHealthCheck>(
            "memory",
            failureStatus: HealthStatus.Degraded
        );

    // Configurar OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: "CryptoMonitor.API",
                        serviceVersion: "1.0.0")
            .AddAttributes(new List<KeyValuePair<string, object>>
            {
                new("deployment.environment", builder.Environment.EnvironmentName)
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("exception.message", exception.Message);
                    activity.SetTag("exception.stacktrace", exception.StackTrace);
                };
            })
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(jaegerOptions =>
            {
                jaegerOptions.AgentHost = builder.Configuration["Jaeger:AgentHost"] ?? "localhost";
                jaegerOptions.AgentPort = int.Parse(builder.Configuration["Jaeger:AgentPort"] ?? "6831");
                jaegerOptions.MaxPayloadSizeInBytes = 4096;
                jaegerOptions.ExportProcessorType = ExportProcessorType.Batch;
                jaegerOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                {
                    MaxQueueSize = 2048,
                    ScheduledDelayMilliseconds = 5000,
                    ExporterTimeoutMilliseconds = 30000,
                    MaxExportBatchSize = 512,
                };
            }))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter());

    // Configurar logging com OpenTelemetry
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.ParseStateValues = true;
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("CryptoMonitor.API"));
    });

    // Configurar Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "CryptoMonitor API", Version = "v1" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.EnableAnnotations();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy => policy
                .WithOrigins("http://localhost:4200") 
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
    builder.Services.AddControllers();
    builder.Host.UseSerilog();

    var app = builder.Build();
    app.UseCors("AllowFrontend");

    // Configurar pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Health Check endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Results = report.Entries.Select(e => new
                {
                    Key = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = e.Value.Duration,
                    Exception = e.Value.Exception?.Message,
                    Data = e.Value.Data
                })
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Name == "database"
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Prometheus endpoint
    app.UseOpenTelemetryPrometheusScrapingEndpoint(context =>
        context.Request.Path == "/metrics" && context.Connection.LocalPort == 8080);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Health Check personalizado para memória
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _threshold = 1024 * 1024 * 100; // 100MB

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemory = GC.GetTotalMemory(false);

        var data = new Dictionary<string, object>
        {
            { "TotalMemory", totalMemory },
            { "TotalAllocated", memoryInfo.TotalCommittedBytes },
            { "HeapSize", memoryInfo.HeapSizeBytes },
            { "MemoryLoad", memoryInfo.MemoryLoadBytes }
        };

        var status = totalMemory > _threshold
            ? HealthStatus.Degraded
            : HealthStatus.Healthy;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: status == HealthStatus.Healthy
                ? "Memory usage is normal"
                : "High memory usage detected",
            exception: null,
            data: data));
    }
}

// Health Check personalizado para DbContext
public class DbContextHealthCheck<TContext> : IHealthCheck where TContext : DbContext
{
    private readonly TContext _dbContext;

    public DbContextHealthCheck(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}
