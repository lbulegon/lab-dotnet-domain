using System.Text.RegularExpressions;

namespace Desafio.Umbler.Helpers
{
    public static class DomainValidator
    {
        public static (bool IsValid, string ErrorMessage, string NormalizedDomain) ValidateDomain(string domain)
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
    }
}

