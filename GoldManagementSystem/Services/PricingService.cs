using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Services
{
    public sealed class PublishedPrice
    {
        public PriceBook Book { get; init; }
        public PriceVersion Version { get; init; }
        public PriceLine Line { get; init; }
    }

    public interface IPricingService
    {
        Task<PublishedPrice> GetPublishedPriceAsync(int productId, int branchId, DateTime? at = null, CancellationToken cancellationToken = default);
    }

    public sealed class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PublishedPrice> GetPublishedPriceAsync(int productId, int branchId, DateTime? at = null, CancellationToken cancellationToken = default)
        {
            var instant = at ?? DateTime.UtcNow;
            var productScope = await _context.Products.AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => product.ProductLegalClass == ProductLegalClassOptions.GoldBarRegulated ? PriceBookScopeOptions.GoldBar : PriceBookScopeOptions.General)
                .FirstOrDefaultAsync(cancellationToken);
            if (productScope == null) return null;
            var books = _context.PriceBooks
                .AsNoTracking()
                .Include(book => book.Versions)
                    .ThenInclude(version => version.Lines)
                .Where(book => book.Status == PriceBookStatusOptions.Published
                    && book.Scope == productScope
                    && book.EffectiveFrom <= instant
                    && (!book.EffectiveTo.HasValue || book.EffectiveTo > instant)
                    && (book.BranchId == branchId || book.BranchId == null))
                .OrderByDescending(book => book.BranchId == branchId)
                .ThenByDescending(book => book.EffectiveFrom);

            foreach (var book in await books.ToListAsync(cancellationToken))
            {
                var version = book.Versions
                    .Where(item => item.EffectiveFrom <= instant && (!item.EffectiveTo.HasValue || item.EffectiveTo > instant))
                    .OrderByDescending(item => item.EffectiveFrom)
                    .FirstOrDefault();
                var line = version?.Lines.FirstOrDefault(item => item.ProductId == productId && item.IsActive);
                if (version != null && line != null)
                {
                    return new PublishedPrice { Book = book, Version = version, Line = line };
                }
            }

            return null;
        }
    }
}
