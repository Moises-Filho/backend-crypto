FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos .csproj primeiro
COPY ["backend/CryptoMonitor.Api/CryptoMonitor.Api.csproj", "backend/CryptoMonitor.Api/"]
COPY ["backend/CryptoMonitor.Core/CryptoMonitor.Core.csproj", "backend/CryptoMonitor.Core/"]
COPY ["backend/CryptoMonitor.Infrastructure/CryptoMonitor.Infrastructure.csproj", "backend/CryptoMonitor.Infrastructure/"]
COPY ["backend/CryptoMonitor.Services/CryptoMonitor.Services.csproj", "backend/CryptoMonitor.Services/"]

# Restaurar dependências
RUN dotnet restore "backend/CryptoMonitor.Api/CryptoMonitor.Api.csproj"

# Copiar todo o código fonte
COPY . .

# Buildar a aplicação
RUN dotnet build "backend/CryptoMonitor.Api/CryptoMonitor.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "backend/CryptoMonitor.Api/CryptoMonitor.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoMonitor.Api.dll"]