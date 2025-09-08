using CryptoMonitor.Core.Interfaces;
using CryptoMonitor.Core.Models; // supondo que o CryptoPrice está em Models
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de preços de criptomoedas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly ICryptoPriceService _cryptoPriceService;
        private readonly ILogger<CryptoController> _logger;

        public CryptoController(ICryptoPriceService cryptoPriceService, ILogger<CryptoController> logger)
        {
            _cryptoPriceService = cryptoPriceService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém os preços atuais das criptomoedas especificadas.
        /// </summary>
        /// <param name="coins">Lista de IDs das criptomoedas (ex: bitcoin, ethereum, cardano).</param>
        /// <returns>Lista de preços das criptomoedas solicitadas.</returns>
        /// <response code="200">Retorna os preços das criptomoedas.</response>
        /// <response code="400">Se a lista de moedas for nula ou vazia.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("prices")]
        [ProducesResponseType(typeof(IEnumerable<CryptoPrice>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrices([FromQuery] string[] coins)
        {
            if (coins == null || coins.Length == 0)
            {
                return BadRequest("É necessário informar pelo menos uma criptomoeda.");
            }

            try
            {
                _logger.LogInformation("Getting prices for coins: {Coins}", string.Join(", ", coins));
                var prices = await _cryptoPriceService.GetCurrentPricesAsync(coins);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting crypto prices");
                return StatusCode(500, "Error retrieving crypto prices");
            }
        }

        /// <summary>
        /// Atualiza os preços das criptomoedas no banco de dados.
        /// </summary>
        /// <param name="coins">Lista de IDs das criptomoedas para atualizar.</param>
        /// <returns>Resultado da operação de atualização.</returns>
        /// <response code="200">Preços atualizados com sucesso.</response>
        /// <response code="400">Se a lista de moedas for nula ou vazia.</response>
        /// <response code="500">Erro ao atualizar preços.</response>
        [HttpPost("update")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePrices([FromQuery] string[] coins)
        {
            if (coins == null || coins.Length == 0)
            {
                return BadRequest("É necessário informar pelo menos uma criptomoeda.");
            }

            try
            {
                _logger.LogInformation("Updating prices for coins: {Coins}", string.Join(", ", coins));
                var prices = await _cryptoPriceService.GetCurrentPricesAsync(coins);
                await _cryptoPriceService.SavePricesToDatabaseAsync(prices);

                return Ok(new
                {
                    Message = "Prices updated successfully",
                    Count = prices.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating crypto prices");
                return StatusCode(500, "Error updating crypto prices");
            }
        }
    }
}
