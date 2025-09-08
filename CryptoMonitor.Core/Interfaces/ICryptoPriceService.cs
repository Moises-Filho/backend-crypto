using CryptoMonitor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoMonitor.Core.Interfaces
{
    public interface ICryptoPriceService
    {
        Task<IEnumerable<CryptoPrice>> GetCurrentPricesAsync(IEnumerable<string> coinIds);
        Task<CryptoPrice> GetPriceHistoryAsync(string coinId, int days = 30);
        Task SavePricesToDatabaseAsync(IEnumerable<CryptoPrice> prices);
    }
}