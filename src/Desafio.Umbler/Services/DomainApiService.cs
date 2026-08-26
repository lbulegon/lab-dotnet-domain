using System.Net.Http;
using System.Threading.Tasks;
using Desafio.Umbler.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Desafio.Umbler.Services
{
    public class DomainApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DomainApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DomainResult> GetDomainAsync(string domainName)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request != null 
                    ? $"{request.Scheme}://{request.Host}" 
                    : "http://localhost:65453";
                
                httpClient.BaseAddress = new System.Uri(baseUrl);
                
                var response = await httpClient.GetAsync($"/api/domain/{domainName}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = JsonSerializer.Deserialize<DomainResult>(content, options);
                    return result ?? new DomainResult();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                    var error = errorResponse.TryGetProperty("error", out var errorProp) 
                        ? errorProp.GetString() 
                        : "Domínio inválido";
                    return new DomainResult 
                    { 
                        Error = error,
                        RequestStatus = 400 
                    };
                }
                else
                {
                    return new DomainResult { Error = "Erro ao consultar domínio" };
                }
            }
            catch (HttpRequestException ex)
            {
                return new DomainResult { Error = "Erro ao comunicar com o servidor: " + ex.Message };
            }
        }
    }
}

