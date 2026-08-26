using System;
using System.Collections.Generic;

namespace Desafio.Umbler.Models
{
    public class DomainResult
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string HostedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? Ttl { get; set; }
        public int? Id { get; set; }
        public List<string> NameServers { get; set; } = new List<string>();
        public string WhoIs { get; set; }
        public string Error { get; set; }
        public int? RequestStatus { get; set; }
    }
}

