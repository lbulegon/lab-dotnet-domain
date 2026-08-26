using System.Net;
using System.Threading.Tasks;

namespace Desafio.Umbler.Services
{
    public interface IDnsService
    {
        Task<DnsQueryResult> QueryAsync(string domain);
    }

    public class DnsQueryResult
    {
        public string IpAddress { get; set; }
        public int Ttl { get; set; }
        public bool HasRecord { get; set; }
    }
}

