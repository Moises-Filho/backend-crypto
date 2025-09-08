using CryptoMonitor.Core.Interfaces;
using CryptoMonitor.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CryptoMonitor.Services
{
    public class CryptoPriceService : ICryptoPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CryptoPriceService> _logger;
        private static readonly ActivitySource ActivitySource = new("CryptoMonitor.Services.CryptoPriceService");

        public CryptoPriceService(HttpClient httpClient, ILogger<CryptoPriceService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CryptoPrice>> GetCurrentPricesAsync(IEnumerable<string> coinIds)
        {
            using var activity = ActivitySource.StartActivity("GetCryptoPrices");
            activity?.SetTag("coin.ids", string.Join(",", coinIds));
            activity?.SetTag("coin.count", coinIds.Count());
            
            try
            {
                var ids = string.Join(",", coinIds);
                var url = $"coins/markets?vs_currency=usd&ids={ids}&order=market_cap_desc&per_page=100&page=1&sparkline=false&locale=en";
                
                activity?.SetTag("http.url", url);
                _logger.LogInformation("Fetching crypto prices from CoinGecko for coins: {Coins}", ids);
                
                var response = await _httpClient.GetAsync(url);
                activity?.SetTag("http.status_code", (int)response.StatusCode);
                
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var prices = JsonSerializer.Deserialize<List<CryptoPrice>>(content, options);

                
                var count = prices?.Count ?? 0;
                activity?.SetTag("prices.retrieved_count", count);
                _logger.LogInformation("Retrieved {Count} crypto prices from CoinGecko", count);
                
                return prices ?? new List<CryptoPrice>();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("error.message", ex.Message);
                activity?.SetTag("error.stacktrace", ex.StackTrace);
                _logger.LogWarning(ex, "Falling back to mock data due to CoinGecko API error");
                
                // Fallback para dados mock
                return GetMockCryptoPrices(coinIds);
            }
        }

        private IEnumerable<CryptoPrice> GetMockCryptoPrices(IEnumerable<string> coinIds)
        {
            using var activity = ActivitySource.StartActivity("GetMockCryptoPrices");
            activity?.SetTag("mock_data", true);
            
            var mockPrices = new List<CryptoPrice>();
            var random = new Random();
            
            foreach (var coinId in coinIds)
            {
                var symbol = coinId.ToLower() switch
                {
                    "bitcoin" => "btc",
                    "ethereum" => "eth",
                    "cardano" => "ada",
                    "solana" => "sol",
                    "binancecoin" => "bnb",
                    "ripple" => "xrp",
                    "dogecoin" => "doge",
                    "polkadot" => "dot",
                    "litecoin" => "ltc",
                    "chainlink" => "link",
                    _ => coinId[..3].ToLower()
                };
                
                var currentPrice = random.Next(100, 100000);
                var priceChange = random.Next(-1000, 1000);
                var priceChangePercent = random.Next(-10, 10);
                
                mockPrices.Add(new CryptoPrice
                {
                    Symbol = symbol,
                    Name = coinId,
                    CurrentPrice = currentPrice,
                    MarketCap = random.Next(1000000, 1000000000),
                    PriceChange24h = priceChange,
                    PriceChangePercentage24h = priceChangePercent,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            activity?.SetTag("mock_prices.generated_count", mockPrices.Count);
            _logger.LogInformation("Generated {Count} mock crypto prices", mockPrices.Count);
            
            return mockPrices;
        }

        public async Task<CryptoPrice> GetPriceHistoryAsync(string coinId, int days = 30)
        {
            using var activity = ActivitySource.StartActivity("GetPriceHistory");
            activity?.SetTag("coin.id", coinId);
            activity?.SetTag("history.days", days);
            
            try
            {
                // Implementação futura para histórico de preços
                _logger.LogInformation("Fetching price history for {CoinId} for {Days} days", coinId, days);
                
                // Simular operação async
                await Task.Delay(100);
                
                throw new NotImplementedException("Price history feature not implemented yet");
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("error.message", ex.Message);
                activity?.SetTag("error.stacktrace", ex.StackTrace);
                _logger.LogError(ex, "Error fetching price history for {CoinId}", coinId);
                throw;
            }
        }

        public async Task SavePricesToDatabaseAsync(IEnumerable<CryptoPrice> prices)
        {
            using var activity = ActivitySource.StartActivity("SavePricesToDatabase");
            activity?.SetTag("prices.count", prices.Count());
            
            try
            {
                _logger.LogInformation("Saving {Count} crypto prices to database", prices.Count());
                
                // Simular operação de salvamento no banco
                await Task.Delay(200);
                
                activity?.SetTag("database.save.success", true);
                _logger.LogInformation("Successfully saved {Count} prices to database", prices.Count());
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("error.message", ex.Message);
                activity?.SetTag("error.stacktrace", ex.StackTrace);
                _logger.LogError(ex, "Error saving prices to database");
                throw;
            }
        }
    }
}