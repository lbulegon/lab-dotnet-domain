using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Models;
using Whois.NET;
using Microsoft.EntityFrameworkCore;
using DnsClient;
using Microsoft.Extensions.Logging;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<DomainController> _logger;

        public DomainController(DatabaseContext db, ILogger<DomainController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            _logger.LogInformation("Iniciando consulta de domínio: {DomainName}", domainName);

            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger.LogWarning("Tentativa de consulta com domínio vazio ou nulo");
                    return BadRequest(new { error = "Nome do domínio é obrigatório" });
                }

                // Validar formato do domínio
                var validationResult = ValidateDomain(domainName);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Domínio com formato inválido: {DomainName}", domainName);
                    return BadRequest(new { error = validationResult.ErrorMessage });
                }

                // Normalizar domínio (remover protocolo, www, etc.)
                domainName = validationResult.NormalizedDomain;

                _logger.LogDebug("Buscando domínio no banco de dados: {DomainName}", domainName);
                var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

                if (domain == null)
                {
                    _logger.LogInformation("Domínio não encontrado no banco. Consultando serviços externos: {DomainName}", domainName);
                    domain = await QueryDomainInfoAsync(domainName);
                    
                    if (domain == null)
                    {
                        _logger.LogWarning("Não foi possível obter informações do domínio: {DomainName}", domainName);
                        return NotFound(new { error = $"Domínio '{domainName}' não encontrado" });
                    }

                    _logger.LogInformation("Domínio consultado com sucesso. IP: {Ip}, HostedAt: {HostedAt}", domain.Ip, domain.HostedAt);
                    _db.Domains.Add(domain);
                }
                else
                {
                    _logger.LogDebug("Domínio encontrado no cache. Verificando TTL...");
                    var timeSinceUpdate = DateTime.Now.Subtract(domain.UpdatedAt).TotalSeconds;
                    _logger.LogDebug("Tempo desde última atualização: {TimeSinceUpdate}s, TTL: {Ttl}s", timeSinceUpdate, domain.Ttl);

                    if (timeSinceUpdate > domain.Ttl)
                    {
                        _logger.LogInformation("TTL expirado. Atualizando informações do domínio: {DomainName}", domainName);
                        var updatedDomain = await QueryDomainInfoAsync(domainName);
                        
                        if (updatedDomain != null)
                        {
                            domain.Name = updatedDomain.Name;
                            domain.Ip = updatedDomain.Ip;
                            domain.UpdatedAt = updatedDomain.UpdatedAt;
                            domain.WhoIs = updatedDomain.WhoIs;
                            domain.Ttl = updatedDomain.Ttl;
                            domain.HostedAt = updatedDomain.HostedAt;
                            _logger.LogInformation("Domínio atualizado com sucesso. Novo IP: {Ip}", domain.Ip);
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao atualizar domínio. Mantendo dados do cache: {DomainName}", domainName);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("TTL ainda válido. Retornando dados do cache");
                    }
                }

                _logger.LogDebug("Salvando alterações no banco de dados");
                await _db.SaveChangesAsync();
                _logger.LogInformation("Consulta de domínio concluída com sucesso: {DomainName}", domainName);

                // Buscar Name Servers para retornar (com timeout para evitar espera longa)
                _logger.LogDebug("Consultando Name Servers para: {DomainName}", domain.Name);
                List<string> nameServers = new List<string>();
                try
                {
                    var lookupOptions = new LookupClientOptions
                    {
                        Timeout = TimeSpan.FromSeconds(5) // Timeout de 5 segundos
                    };
                    var lookup = new LookupClient(lookupOptions);
                    var nsResult = await lookup.QueryAsync(domain.Name, QueryType.NS);
                    nameServers = nsResult.Answers.NsRecords().Select(ns => ns.NSDName.Value).ToList();
                    _logger.LogDebug("Name Servers encontrados: {NameServers}", string.Join(", ", nameServers));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao consultar Name Servers para: {DomainName}. Continuando sem Name Servers.", domain.Name);
                    // Continua sem Name Servers em caso de erro
                }

                // Retornar domain com Name Servers
                var response = new
                {
                    domain.Id,
                    domain.Name,
                    domain.Ip,
                    domain.UpdatedAt,
                    domain.WhoIs,
                    domain.Ttl,
                    domain.HostedAt,
                    NameServers = nameServers
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar consulta do domínio: {DomainName}", domainName);
                return StatusCode(500, new { error = "Erro interno ao processar a requisição", message = ex.Message });
            }
        }

        private (bool IsValid, string ErrorMessage, string NormalizedDomain) ValidateDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return (false, "Nome do domínio é obrigatório", null);
            }

            var trimmedDomain = domain.Trim();

            // Remover protocolo se presente
            trimmedDomain = Regex.Replace(trimmedDomain, @"^https?://", "", RegexOptions.IgnoreCase);

            // Remover www. se presente (opcional)
            trimmedDomain = Regex.Replace(trimmedDomain, @"^www\.", "", RegexOptions.IgnoreCase);

            // Validar se não contém espaços ou tabs
            if (trimmedDomain.Contains(' ') || trimmedDomain.Contains('\t'))
            {
                return (false, "O domínio não pode conter espaços", null);
            }

            // Validar se não começa ou termina com ponto
            if (trimmedDomain.StartsWith(".") || trimmedDomain.EndsWith("."))
            {
                return (false, "O domínio não pode começar ou terminar com ponto", null);
            }

            // Validar se não contém dois pontos consecutivos
            if (trimmedDomain.Contains(".."))
            {
                return (false, "O domínio não pode conter pontos consecutivos", null);
            }

            // Validar que não começa ou termina com hífen
            if (trimmedDomain.StartsWith("-") || trimmedDomain.EndsWith("-"))
            {
                return (false, "O domínio não pode começar ou terminar com hífen", null);
            }

            // Regex para validar formato de domínio completo
            // Deve ter pelo menos: dominio.tld
            var domainPattern = @"^([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,}$";
            
            if (!Regex.IsMatch(trimmedDomain, domainPattern, RegexOptions.IgnoreCase))
            {
                return (false, "Formato de domínio inválido. Por favor, digite um domínio completo (ex: umbler.com)", null);
            }

            // Validar que cada parte do domínio tem formato válido
            var parts = trimmedDomain.Split('.');
            if (parts.Length < 2)
            {
                return (false, "Formato de domínio inválido. O domínio deve ter pelo menos um ponto separando o nome do TLD", null);
            }

            // TLD deve ter pelo menos 2 caracteres
            var tld = parts[parts.Length - 1];
            if (tld.Length < 2)
            {
                return (false, "Formato de domínio inválido. A extensão do domínio deve ter pelo menos 2 caracteres", null);
            }

            // Cada parte do domínio deve ter pelo menos 1 caractere válido
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (string.IsNullOrWhiteSpace(part) || part == "-" || !Regex.IsMatch(part, @"^[a-z0-9-]+$", RegexOptions.IgnoreCase))
                {
                    return (false, "Formato de domínio inválido. Cada parte do domínio deve conter apenas letras, números e hífens", null);
                }
            }

            return (true, null, trimmedDomain);
        }

        private async Task<Domain> QueryDomainInfoAsync(string domainName)
        {
            try
            {
                _logger.LogDebug("Iniciando consulta WHOIS para: {DomainName}", domainName);
                var response = await WhoisClient.QueryAsync(domainName);
                _logger.LogDebug("Consulta WHOIS concluída. Organization: {Organization}", response.OrganizationName);

                _logger.LogDebug("Iniciando consulta DNS para: {DomainName}", domainName);
                var lookupOptions = new LookupClientOptions
                {
                    Timeout = TimeSpan.FromSeconds(10) // Timeout de 10 segundos para consulta DNS completa
                };
                var lookup = new LookupClient(lookupOptions);
                var result = await lookup.QueryAsync(domainName, QueryType.ANY);
                var record = result.Answers.ARecords().FirstOrDefault();
                
                if (record == null)
                {
                    _logger.LogWarning("Nenhum registro A encontrado para o domínio: {DomainName}", domainName);
                    return null;
                }

                var address = record.Address;
                var ip = address?.ToString();
                _logger.LogDebug("Registro A encontrado. IP: {Ip}, TTL: {Ttl}", ip, record.TimeToLive);

                // Extrair Name Servers (NS records)
                var nsRecords = result.Answers.NsRecords().Select(ns => ns.NSDName.Value).ToList();
                _logger.LogDebug("Name Servers encontrados: {NameServers}", string.Join(", ", nsRecords));

                if (string.IsNullOrWhiteSpace(ip))
                {
                    _logger.LogWarning("IP não encontrado para o domínio: {DomainName}", domainName);
                    return null;
                }

                _logger.LogDebug("Consultando WHOIS do IP: {Ip}", ip);
                var hostResponse = await WhoisClient.QueryAsync(ip);
                _logger.LogDebug("Consulta WHOIS do IP concluída. Organization: {Organization}", hostResponse.OrganizationName);

                var domain = new Domain
                {
                    Name = domainName,
                    Ip = ip,
                    UpdatedAt = DateTime.Now,
                    WhoIs = response.Raw,
                    Ttl = record.TimeToLive,
                    HostedAt = hostResponse.OrganizationName
                };

                return domain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar informações do domínio: {DomainName}", domainName);
                throw;
            }
        }
    }
}
