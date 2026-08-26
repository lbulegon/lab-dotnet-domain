using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public interface IWhoisService
    {
        Task<WhoisResponse> QueryAsync(string query);
    }
}

