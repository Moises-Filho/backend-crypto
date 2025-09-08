using System;
using System.Text.Json.Serialization;

namespace CryptoMonitor.Core.Models
{
    /// <summary>
    /// Representa o preço de uma criptomoeda retornado pela API CoinGecko
    /// </summary>
    public class CryptoPrice
    {
        /// <summary>
        /// ID interno do registro
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Símbolo da criptomoeda (ex: BTC, ETH)
        /// </summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo da criptomoeda (ex: Bitcoin, Ethereum)
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Preço atual em USD
        /// </summary>
        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Capitalização de mercado em USD
        /// </summary>
        [JsonPropertyName("market_cap")]
        public decimal MarketCap { get; set; }

        /// <summary>
        /// Variação de preço nas últimas 24h em USD
        /// </summary>
        [JsonPropertyName("price_change_24h")]
        public decimal PriceChange24h { get; set; }

        /// <summary>
        /// Percentual de variação de preço nas últimas 24h
        /// </summary>
        [JsonPropertyName("price_change_percentage_24h")]
        public decimal PriceChangePercentage24h { get; set; }

        /// <summary>
        /// Data e hora da última atualização do preço
        /// </summary>
        [JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Data e hora de criação do registro no banco
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
