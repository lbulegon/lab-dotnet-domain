using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class WhoisService : IWhoisService
    {
        public async Task<WhoisResponse> QueryAsync(string query)
        {
            return await WhoisClient.QueryAsync(query);
        }
    }
}

