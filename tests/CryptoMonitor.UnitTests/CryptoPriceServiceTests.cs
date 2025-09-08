using CryptoMonitor.Core.Models;
using CryptoMonitor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CryptoMonitor.UnitTests.Services
{
    public class CryptoPriceServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<CryptoPriceService>> _loggerMock;
        private readonly CryptoPriceService _cryptoPriceService;

        public CryptoPriceServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
            };
            _loggerMock = new Mock<ILogger<CryptoPriceService>>();
            _cryptoPriceService = new CryptoPriceService(_httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentPricesAsync_ReturnsPrices_WhenApiCallSucceeds()
        {
            // Arrange
            var expectedPrices = new List<CryptoPrice>
            {
                new() { Id = 1, Symbol = "btc", Name = "Bitcoin", CurrentPrice = 50000.00m },
                new() { Id = 2, Symbol = "eth", Name = "Ethereum", CurrentPrice = 3000.00m }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedPrices))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _cryptoPriceService.GetCurrentPricesAsync(new[] { "bitcoin", "ethereum" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Symbol == "btc");
            Assert.Contains(result, p => p.Symbol == "eth");
        }

        [Fact]
        public async Task GetCurrentPricesAsync_ReturnsMockData_WhenApiCallFails()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API call failed"));

            // Act
            var result = await _cryptoPriceService.GetCurrentPricesAsync(new[] { "bitcoin" });

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("btc", result.First().Symbol);
        }

        [Fact]
        public async Task GetCurrentPricesAsync_ReturnsEmpty_WhenNoCoinsProvided()
        {
            // Act
            var result = await _cryptoPriceService.GetCurrentPricesAsync(Array.Empty<string>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetMockCryptoPrices_ReturnsCorrectSymbols()
        {
            // Arrange
            var coinIds = new[] { "bitcoin", "ethereum", "cardano", "solana" };

            // Act
            var result = _cryptoPriceService.GetType()
                .GetMethod("GetMockCryptoPrices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_cryptoPriceService, new object[] { coinIds }) as IEnumerable<CryptoPrice>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
            Assert.Contains(result, p => p.Symbol == "btc");
            Assert.Contains(result, p => p.Symbol == "eth");
            Assert.Contains(result, p => p.Symbol == "ada");
            Assert.Contains(result, p => p.Symbol == "sol");
        }
    }
}