using CryptoMonitor.Core.Models;
using Xunit;

namespace CryptoMonitor.UnitTests.Models
{
    public class CryptoPriceTests
    {
        [Fact]
        public void CryptoPrice_Properties_SetCorrectly()
        {
            // Arrange & Act
            var cryptoPrice = new CryptoPrice
            {
                Id = 1,
                Symbol = "btc",
                Name = "Bitcoin",
                CurrentPrice = 50000.50m,
                MarketCap = 1000000000.00m,
                PriceChange24h = 500.25m,
                PriceChangePercentage24h = 1.5m,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal("btc", cryptoPrice.Symbol);
            Assert.Equal("Bitcoin", cryptoPrice.Name);
            Assert.Equal(50000.50m, cryptoPrice.CurrentPrice);
            Assert.Equal(1000000000.00m, cryptoPrice.MarketCap);
            Assert.Equal(500.25m, cryptoPrice.PriceChange24h);
            Assert.Equal(1.5m, cryptoPrice.PriceChangePercentage24h);
        }

        [Fact]
        public void CryptoPrice_DefaultValues_AreSet()
        {
            // Act
            var cryptoPrice = new CryptoPrice();

            // Assert
            Assert.Equal(0, cryptoPrice.Id);
            Assert.Equal(string.Empty, cryptoPrice.Symbol);
            Assert.Equal(string.Empty, cryptoPrice.Name);
            Assert.Equal(0m, cryptoPrice.CurrentPrice);
            Assert.Equal(0m, cryptoPrice.MarketCap);
            Assert.Equal(0m, cryptoPrice.PriceChange24h);
            Assert.Equal(0m, cryptoPrice.PriceChangePercentage24h);
            Assert.True(cryptoPrice.CreatedAt <= DateTime.UtcNow);
        }
    }
}