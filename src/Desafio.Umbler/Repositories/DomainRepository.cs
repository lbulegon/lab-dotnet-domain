using System.Linq;
using System.Threading.Tasks;
using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Umbler.Repositories
{
    public class DomainRepository : IDomainRepository
    {
        private readonly DatabaseContext _context;

        public DomainRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Domain> GetByNameAsync(string domainName)
        {
            return await _context.Domains
                .FirstOrDefaultAsync(d => d.Name == domainName);
        }

        public async Task<Domain> AddAsync(Domain domain)
        {
            await _context.Domains.AddAsync(domain);
            return domain;
        }

        public Task UpdateAsync(Domain domain)
        {
            _context.Domains.Update(domain);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

