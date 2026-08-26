# 📋 Como Consultar os Logs da Aplicação

## 🔍 Visualização dos Logs

### 1. Console/Terminal (Método Principal)

Os logs aparecem automaticamente no **console/terminal** onde a aplicação está rodando via `dotnet run`.

**Como visualizar:**
1. Execute a aplicação:
   ```bash
   cd src/Desafio.Umbler
   dotnet run
   ```

2. Os logs aparecerão no console em tempo real enquanto a aplicação processa requisições.

**Exemplo de log no console:**
```
info: Desafio.Umbler.Controllers.DomainController[0]
      Iniciando consulta de domínio: umbler.com
dbug: Desafio.Umbler.Controllers.DomainController[0]
      Buscando domínio no banco de dados: umbler.com
info: Desafio.Umbler.Controllers.DomainController[0]
      Domínio não encontrado no banco. Consultando serviços externos: umbler.com
```

---

## 📊 Níveis de Log Configurados

### Ambiente de Desenvolvimento (`appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Desafio.Umbler": "Debug"
    }
  }
}
```

**Níveis disponíveis:**
- ✅ **Debug** - Informações detalhadas (inclui todos os logs)
- ✅ **Information** - Informações gerais sobre o fluxo
- ✅ **Warning** - Avisos (domínio não encontrado, etc.)
- ✅ **Error** - Erros e exceções

### Ambiente de Produção (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Desafio.Umbler": "Information"
    }
  }
}
```

---

## 📝 Tipos de Logs Implementados

### Logs de Informação (Information)
```csharp
_logger.LogInformation("Iniciando consulta de domínio: {DomainName}", domainName);
_logger.LogInformation("Domínio consultado com sucesso. IP: {Ip}, HostedAt: {HostedAt}", domain.Ip, domain.HostedAt);
```

### Logs de Debug
```csharp
_logger.LogDebug("Buscando domínio no banco de dados: {DomainName}", domainName);
_logger.LogDebug("Tempo desde última atualização: {TimeSinceUpdate}s, TTL: {Ttl}s", timeSinceUpdate, domain.Ttl);
```

### Logs de Aviso (Warning)
```csharp
_logger.LogWarning("Tentativa de consulta com domínio vazio ou nulo");
_logger.LogWarning("Não foi possível obter informações do domínio: {DomainName}", domainName);
```

### Logs de Erro (Error)
```csharp
_logger.LogError(ex, "Erro ao processar consulta do domínio: {DomainName}", domainName);
_logger.LogError(ex, "Erro ao consultar informações do domínio: {DomainName}", domainName);
```

---

## 🔍 Pontos de Log Implementados

### DomainController

1. **Início de requisição**
   - Log: Information - "Iniciando consulta de domínio: {DomainName}"

2. **Busca no banco de dados**
   - Log: Debug - "Buscando domínio no banco de dados: {DomainName}"

3. **Domínio não encontrado**
   - Log: Information - "Domínio não encontrado no banco. Consultando serviços externos"

4. **Verificação de TTL**
   - Log: Debug - "Domínio encontrado no cache. Verificando TTL..."
   - Log: Debug - "Tempo desde última atualização: {TimeSinceUpdate}s, TTL: {Ttl}s"
   - Log: Information - "TTL expirado. Atualizando informações do domínio"

5. **Consultas DNS/WHOIS**
   - Log: Debug - "Iniciando consulta WHOIS para: {DomainName}"
   - Log: Debug - "Iniciando consulta DNS para: {DomainName}"
   - Log: Debug - "Registro A encontrado. IP: {Ip}, TTL: {Ttl}"

6. **Sucesso**
   - Log: Information - "Domínio consultado com sucesso. IP: {Ip}, HostedAt: {HostedAt}"
   - Log: Information - "Consulta de domínio concluída com sucesso: {DomainName}"

7. **Erros e Avisos**
   - Log: Warning - "Tentativa de consulta com domínio vazio ou nulo"
   - Log: Warning - "Nenhum registro A encontrado para o domínio"
   - Log: Error - "Erro ao processar consulta do domínio"

---

## 💻 Comandos Úteis

### Visualizar logs em tempo real (Windows PowerShell)
```powershell
# Se a aplicação estiver rodando em background, você pode verificar os processos
Get-Process -Name "Desafio.Umbler" | Select-Object Id, ProcessName, StartTime
```

### Filtrar logs no console
Quando estiver rodando, você pode filtrar visualmente por:
- `info:` - Informações gerais
- `dbug:` - Debug (apenas em desenvolvimento)
- `warn:` - Avisos
- `fail:` ou `erro:` - Erros

### Redirecionar logs para arquivo (opcional)
```powershell
dotnet run > logs.txt 2>&1
```

---

## 📁 Salvar Logs em Arquivo (Opcional)

Se você quiser salvar os logs em arquivo, pode modificar o `Program.cs`:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddFile("logs/app-{Date}.txt", LogLevel.Debug);
        })
        // ... resto da configuração
```

**Nota:** Seria necessário instalar o pacote `Serilog.Extensions.Logging.File` ou similar.

---

## 🎯 Dicas para Debug

1. **Ver logs detalhados:**
   - Certifique-se de estar rodando em ambiente de **Development**
   - Os logs Debug aparecem apenas neste ambiente

2. **Procurar erros:**
   - Procure por `fail:` ou `erro:` no console
   - Logs de erro incluem stack traces completos

3. **Rastrear uma requisição específica:**
   - Procure pelo nome do domínio nos logs
   - Todos os logs relacionados incluem `{DomainName}`

4. **Verificar performance:**
   - Observe os timestamps dos logs
   - Compare tempos entre "Iniciando" e "Concluída com sucesso"

---

## 📌 Logs do Entity Framework

O Entity Framework também está configurado para logar no console:

```csharp
.LogTo(Console.WriteLine, LogLevel.Information)
```

Isso mostra:
- Queries SQL executadas
- Tempo de execução
- Parâmetros (quando EnableSensitiveDataLogging estiver ativo)

**Exemplo:**
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (223ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT `Domain`.`Id`, `Domain`.`Name`, `Domain`.`Ip`, ...
      FROM `Domains` AS `Domain`
      WHERE `Domain`.`Name` = @__domainName_0
```

---

## 🔐 Segurança

⚠️ **Importante:** Em produção, desative:
- `EnableSensitiveDataLogging()` (pode expor dados sensíveis)
- `EnableDetailedErrors()` (pode expor detalhes de implementação)
- Logs em nível Debug (use Information ou superior)

---

## 📞 Próximos Passos

Se você precisar de mais funcionalidades de logging:
- Salvar logs em arquivo
- Enviar logs para serviços externos (Application Insights, etc.)
- Filtrar logs por categoria
- Agrupar logs por requisição

Avise se precisar implementar alguma dessas funcionalidades!

