# 🏗️ Tarefa: Completar Arquitetura em Camadas

**Status:** 📅 Pendente  
**Prioridade:** Alta  
**Estimativa:** 1h30 - 2h15  
**Complexidade:** Média

---

## 📋 Resumo

Completar a implementação da arquitetura em camadas movendo a lógica de negócio do `DomainController` para a camada de serviços, implementando o `DomainService` e configurando a injeção de dependência.

**Progresso Atual:** ~70% concluído

---

## ✅ O que já foi implementado

### Interfaces Criadas
- ✅ `IWhoisService` - Abstração para consultas WHOIS
- ✅ `IDnsService` - Abstração para consultas DNS
- ✅ `IDomainRepository` - Abstração para acesso a dados

### Implementações Criadas
- ✅ `WhoisService` - Wrapper para WhoisClient (permite mock)
- ✅ `DnsService` - Wrapper para LookupClient (permite mock)
- ✅ `DomainRepository` - Implementação do Repository Pattern

### Outros Componentes
- ✅ `DomainViewModel` - DTO para retorno da API
- ✅ `DomainValidator` - Helper para validação de domínio

---

## ⏳ O que precisa ser implementado

### 1. Criar `IDomainService` e `DomainService`

**Esforço:** Baixo-Médio  
**Tempo Estimado:** 30-45 minutos  
**Complexidade:** Média

#### Arquivos a Criar

**`Services/IDomainService.cs`**
```csharp
public interface IDomainService
{
    Task<Domain> GetDomainInfoAsync(string domainName);
}
```

**`Services/DomainService.cs`**

**Responsabilidades:**
- Orquestrar consultas DNS e WHOIS através de `IDnsService` e `IWhoisService`
- Gerenciar lógica de cache baseada em TTL através de `IDomainRepository`
- Criar e atualizar entidades `Domain`
- Validar dados
- Consultar Name Servers (extrair lógica que está no controller)

**Dependências a Injetar:**
- `IDnsService`
- `IWhoisService`
- `IDomainRepository`
- `ILogger<DomainService>`

**Métodos Principais:**
1. `GetDomainInfoAsync(string domainName)` - Método principal que orquestra todo o fluxo:
   - Buscar no repositório
   - Verificar TTL
   - Consultar serviços externos se necessário
   - Atualizar/criar no repositório
   - Consultar Name Servers
   - Retornar `Domain` com Name Servers incluídos

2. `QueryDomainInfoAsync(string domainName)` - Consultar serviços externos (DNS + WHOIS)
   - Consultar WHOIS do domínio
   - Consultar DNS (registro A) com timeout de 10 segundos
   - Consultar WHOIS do IP encontrado
   - Criar e retornar entidade `Domain`

3. `GetNameServersAsync(string domainName)` - Consultar Name Servers (com timeout de 5 segundos)
   - Retornar lista de Name Servers
   - Em caso de erro/timeout, retornar lista vazia (não deve bloquear resposta)

**Nota:** O `DnsService` atual não tem timeout configurado. Considerar adicionar timeout no `DnsService` ou criar método específico no service para consulta com timeout.

---

### 2. Configurar Injeção de Dependência no `Startup.cs`

**Esforço:** Baixo  
**Tempo Estimado:** 5-10 minutos  
**Complexidade:** Baixa

#### Arquivo a Modificar
- `Startup.cs`

#### Configuração a Adicionar em `ConfigureServices`:

```csharp
// Serviços
services.AddScoped<IWhoisService, WhoisService>();
services.AddScoped<IDnsService, DnsService>();
services.AddScoped<IDomainService, DomainService>();

// Repositórios
services.AddScoped<IDomainRepository, DomainRepository>();
```

**Nota:** Usar `AddScoped` para todos, pois são operações por requisição HTTP.

---

### 3. Refatorar `DomainController`

**Esforço:** Médio  
**Tempo Estimado:** 45-60 minutos  
**Complexidade:** Média-Alta

#### Arquivo a Modificar
- `Controllers/DomainController.cs`

#### Mudanças Necessárias:

1. **Remover dependências:**
   - ❌ `DatabaseContext _db` - usar `IDomainRepository` via service
   - ❌ `WhoisClient.QueryAsync` - usar `IWhoisService` via service
   - ❌ `LookupClient` - usar `IDnsService` via service

2. **Adicionar dependências:**
   - ✅ `IDomainService _domainService`
   - ✅ Manter `ILogger<DomainController>`

3. **Simplificar método `Get`:**
   ```csharp
   [HttpGet, Route("domain/{domainName}")]
   public async Task<IActionResult> Get(string domainName)
   {
       _logger.LogInformation("Iniciando consulta de domínio: {DomainName}", domainName);
       
       try
       {
           // Validação básica (ou mover para service)
           if (string.IsNullOrWhiteSpace(domainName))
           {
               return BadRequest(new { error = "Nome do domínio é obrigatório" });
           }
           
           // Validar formato (usar DomainValidator helper)
           var validationResult = DomainValidator.ValidateDomain(domainName);
           if (!validationResult.IsValid)
           {
               return BadRequest(new { error = validationResult.ErrorMessage });
           }
           
           domainName = validationResult.NormalizedDomain;
           
           // Usar service para buscar informações
           var domain = await _domainService.GetDomainInfoAsync(domainName);
           
           if (domain == null)
           {
               return NotFound(new { error = $"Domínio '{domainName}' não encontrado" });
           }
           
           // Mapear para ViewModel (incluindo Name Servers)
           var viewModel = MapToViewModel(domain);
           
           return Ok(viewModel);
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Erro ao processar consulta do domínio: {DomainName}", domainName);
           return StatusCode(500, new { error = "Erro interno ao processar a requisição" });
       }
   }
   ```

4. **Métodos a Remover:**
   - ❌ `QueryDomainInfoAsync` - mover para `DomainService`
   - ❌ Lógica de Name Servers (mover para `DomainService`)

5. **Métodos a Manter/Adicionar:**
   - ✅ `ValidateDomain` - ou usar `DomainValidator` helper (já existe)
   - ✅ `MapToViewModel` - criar método privado para mapear `Domain` → `DomainViewModel` com Name Servers

6. **Retornar ViewModel Completo:**
   - Incluir Name Servers no ViewModel ou criar extensão que inclua Name Servers na resposta

---

### 4. Ajustar `DomainViewModel` para incluir Name Servers

**Esforço:** Baixo  
**Tempo Estimado:** 10-15 minutos  
**Complexidade:** Baixa

#### Opção 1: Adicionar NameServers ao ViewModel existente
```csharp
public class DomainViewModel
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public string HostedAt { get; set; }
    public List<string> NameServers { get; set; } = new List<string>();
}
```

#### Opção 2: Criar ViewModel estendido apenas para API
```csharp
public class DomainApiViewModel : DomainViewModel
{
    public DateTime? UpdatedAt { get; set; }
    public int? Ttl { get; set; }
    public int? Id { get; set; }
    public List<string> NameServers { get; set; }
    public string WhoIs { get; set; } // Opcional, se necessário
}
```

**Recomendação:** Usar Opção 1 se Name Servers forem sempre necessários, ou Opção 2 se quiser manter o ViewModel básico e estender quando necessário.

---

### 5. Ajustes Finais

**Esforço:** Baixo  
**Tempo Estimado:** 15-20 minutos  
**Complexidade:** Baixa

#### Checklist:
- [ ] Verificar que todos os campos estão sendo retornados corretamente
- [ ] Testar fluxo completo (domínio novo, domínio em cache, TTL expirado)
- [ ] Verificar logs estão funcionando corretamente
- [ ] Garantir que Name Servers estão sendo retornados
- [ ] Testar tratamento de erros
- [ ] Compilar e verificar se não há erros

---

## 📊 Estimativa Detalhada

| Tarefa | Tempo | Complexidade | Risco |
|--------|-------|--------------|-------|
| 1. Criar DomainService | 30-45 min | Média | Baixo |
| 2. Configurar DI | 5-10 min | Baixa | Muito Baixo |
| 3. Refatorar Controller | 45-60 min | Média-Alta | Médio |
| 4. Ajustar ViewModel | 10-15 min | Baixa | Baixo |
| 5. Ajustes finais | 15-20 min | Baixa | Baixo |
| **TOTAL** | **1h45 - 2h30** | **Média** | **Médio** |

---

## ⚠️ Riscos Identificados

1. **Médio:** Migrar lógica de Name Servers para service
   - **Mitigação:** Testar cuidadosamente a extração da lógica

2. **Baixo:** Garantir que todos os campos sejam mapeados corretamente
   - **Mitigação:** Criar método de mapeamento e testar

3. **Baixo:** Manter compatibilidade com frontend (Blazor)
   - **Mitigação:** Verificar estrutura de resposta JSON

---

## ✅ Benefícios Esperados

1. **Redução de Complexidade**
   - Controller simplificado (apenas orquestração HTTP)
   - Complexidade ciclomática reduzida

2. **Testabilidade**
   - Possibilidade de mockar todas as dependências
   - Permite implementar teste obrigatório `Domain_Moking_WhoisClient()`

3. **Manutenibilidade**
   - Código mais organizado
   - Separação clara de responsabilidades
   - Facilita futuras extensões

4. **Reutilização**
   - Lógica de negócio pode ser reutilizada em outros contextos
   - Services podem ser testados independentemente

---

## 📝 Notas Técnicas

### Estrutura de Dependências Esperada

```
DomainController
    ↓
IDomainService
    ↓
    ├── IDnsService (consultas DNS)
    ├── IWhoisService (consultas WHOIS)
    └── IDomainRepository (acesso a dados)
```

### Fluxo de Dados

1. **Requisição HTTP** → `DomainController.Get()`
2. **Validação** → `DomainValidator.ValidateDomain()`
3. **Service** → `DomainService.GetDomainInfoAsync()`
4. **Repository** → Verifica cache no banco
5. **Services** → Consulta DNS/WHOIS se necessário
6. **Repository** → Salva/atualiza no banco
7. **Mapeamento** → `Domain` → `DomainViewModel`
8. **Resposta HTTP** → Retorna ViewModel

### Ciclo de Vida dos Serviços

- **AddScoped:** Uma instância por requisição HTTP
- Apropriado para operações com banco de dados
- Melhor performance que Transient
- Permite compartilhar estado durante o request

---

## 🔍 Referências

- Arquivo original da tarefa: `docs/TAREFAS_SOLICITADAS.md` (linhas 68-83)
- Análise detalhada: `docs/ANALISE_IMPLEMENTACAO_VS_REQUISITOS.md`
- Melhorias implementadas: `docs/MELHORIAS_IMPLEMENTADAS.md`

---

## 📌 Observações

- A validação de domínio já está no helper `DomainValidator`, então pode ser reutilizada
- O controller atual tem ~280 linhas, após refatoração deve ter ~80-100 linhas
- Lógica de Name Servers precisa ser extraída do controller (linhas 104-122)

---

**Última Atualização:** 18/12/2025  
**Criado por:** Análise automática do projeto

