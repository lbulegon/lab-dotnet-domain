using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;

namespace Desafio.Umbler.Services
{
    public class DnsService : IDnsService
    {
        public async Task<DnsQueryResult> QueryAsync(string domain)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.ANY);
            var record = result.Answers.ARecords().FirstOrDefault();

            if (record == null)
            {
                return new DnsQueryResult
                {
                    HasRecord = false,
                    IpAddress = null,
                    Ttl = 0
                };
            }

            var address = record.Address;
            var ip = address?.ToString();

            return new DnsQueryResult
            {
                HasRecord = !string.IsNullOrWhiteSpace(ip),
                IpAddress = ip,
                Ttl = record.TimeToLive
            };
        }
    }
}

