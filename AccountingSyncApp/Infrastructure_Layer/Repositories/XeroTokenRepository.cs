using Domain_Layer.Models;
using Application_Layer.Interfaces_Repository;
using Infrastructure_Layer.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories
{
    public class XeroTokenRepository : IXeroTokenRepository
    {
        private readonly AccountingDbContext _context;

        public XeroTokenRepository(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<XeroTokenResponse?> GetTokenAsync()
        {
            // Returns the most recently saved token
            return await _context.XeroTokensResponse
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task SaveTokenAsync(XeroTokenResponse token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            // Optional: update UpdatedAt
            token.UpdatedAt = DateTime.UtcNow;

            // Add new token record
            _context.XeroTokensResponse.Add(token);
            await _context.SaveChangesAsync();
        }
    }
}
