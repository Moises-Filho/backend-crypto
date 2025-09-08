using CryptoMonitor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoMonitor.Infrastructure.Data  // Adicionar .Data no namespace
{
    public class CryptoDbContext : DbContext
    {
        public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options)
        {
        }

        public DbSet<CryptoPrice> CryptoPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CryptoPrice>(entity =>
            {
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.LastUpdated);
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18, 8)");
                entity.Property(e => e.MarketCap).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.PriceChange24h).HasColumnType("decimal(18, 8)");
                entity.Property(e => e.PriceChangePercentage24h).HasColumnType("decimal(18, 8)");
            });
        }
    }
}