using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CryptoMonitor.Api.Controllers
{
    /// <summary>
    /// Controller para monitoramento de saúde da aplicação
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica a saúde completa da aplicação
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Health check requested");

            var report = await _healthCheckService.CheckHealthAsync();

            var response = new HealthResponse
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.ToDictionary(
                    e => e.Key,
                    e => new HealthEntry
                    {
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration,
                        Exception = e.Value.Exception?.Message,
                        Data = e.Value.Data
                    })
            };

            return report.Status == HealthStatus.Healthy
                ? Ok(response)
                : StatusCode(503, response);
        }

        /// <summary>
        /// Verificação simples de saúde (ping)
        /// </summary>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            _logger.LogInformation("Ping health check requested");
            return Ok(new { status = "OK", message = "API is running", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Verifica a saúde do banco de dados
        /// </summary>
        [HttpGet("database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckDatabase()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var dbEntry = report.Entries.FirstOrDefault(e => e.Key.ToLower().Contains("database"));

            if (!default(KeyValuePair<string, HealthReportEntry>).Equals(dbEntry))
            {
                var entry = new HealthEntry
                {
                    Status = dbEntry.Value.Status.ToString(),
                    Description = dbEntry.Value.Description,
                    Duration = dbEntry.Value.Duration,
                    Exception = dbEntry.Value.Exception?.Message,
                    Data = dbEntry.Value.Data
                };

                return dbEntry.Value.Status == HealthStatus.Healthy
                    ? Ok(entry)
                    : StatusCode(503, entry);
            }

            return StatusCode(503, new { status = "Unhealthy", message = "Database health check not found" });
        }
    }

    /// <summary>
    /// Response padrão para health checks
    /// </summary>
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public TimeSpan TotalDuration { get; set; }
        public Dictionary<string, HealthEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Entrada individual de health check
    /// </summary>
    public class HealthEntry
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Exception { get; set; }
        public IReadOnlyDictionary<string, object>? Data { get; set; }
    }
}
