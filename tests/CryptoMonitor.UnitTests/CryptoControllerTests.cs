using CryptoMonitor.Api.Controllers;
using CryptoMonitor.Core.Interfaces;
using CryptoMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CryptoMonitor.UnitTests.Controllers
{
    public class CryptoControllerTests
    {
        private readonly Mock<ICryptoPriceService> _cryptoPriceServiceMock;
        private readonly Mock<ILogger<CryptoController>> _loggerMock;
        private readonly CryptoController _controller;

        public CryptoControllerTests()
        {
            _cryptoPriceServiceMock = new Mock<ICryptoPriceService>();
            _loggerMock = new Mock<ILogger<CryptoController>>();
            _controller = new CryptoController(_cryptoPriceServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetPrices_ReturnsOk_WithPrices()
        {
            // Arrange
            var expectedPrices = new List<CryptoPrice>
            {
                new() { Symbol = "btc", Name = "Bitcoin", CurrentPrice = 50000.00m },
                new() { Symbol = "eth", Name = "Ethereum", CurrentPrice = 3000.00m }
            };

            _cryptoPriceServiceMock.Setup(service => service.GetCurrentPricesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(expectedPrices);

            // Act
            var result = await _controller.GetPrices(new[] { "bitcoin", "ethereum" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPrices = Assert.IsAssignableFrom<IEnumerable<CryptoPrice>>(okResult.Value);
            Assert.Equal(2, returnedPrices.Count());
        }

        [Fact]
        public async Task GetPrices_ReturnsInternalServerError_WhenServiceThrows()
        {
            // Arrange
            _cryptoPriceServiceMock.Setup(service => service.GetCurrentPricesAsync(It.IsAny<IEnumerable<string>>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetPrices(new[] { "bitcoin" });

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePrices_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var expectedPrices = new List<CryptoPrice>
            {
                new() { Symbol = "btc", Name = "Bitcoin", CurrentPrice = 50000.00m }
            };

            _cryptoPriceServiceMock.Setup(service => service.GetCurrentPricesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(expectedPrices);

            _cryptoPriceServiceMock.Setup(service => service.SavePricesToDatabaseAsync(It.IsAny<IEnumerable<CryptoPrice>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdatePrices(new[] { "bitcoin" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}