# 📋 Avaliação Geral do Projeto - Desafio Umbler

**Data da Avaliação:** Data da análise inicial  
**Versão do Projeto:** Análise Inicial  
**Framework:** ASP.NET Core 6.0

---

## 🎯 Visão Geral

Esta é uma aplicação web que consulta informações DNS e WHOIS de domínios. O sistema faz cache das consultas em banco de dados MySQL utilizando TTL (Time To Live) para determinar quando os dados devem ser atualizados.

**Funcionalidade Principal:**
- Recebe um domínio como entrada
- Consulta informações DNS (registro A, name servers)
- Consulta informações WHOIS (organização hospedadora)
- Armazena resultados em cache considerando TTL

---

## 🏗️ Arquitetura e Estrutura do Projeto

### **Estrutura de Diretórios**

```
dev-challenge/
├── Desafio.Umbler.sln
├── README.md
└── src/
    ├── Desafio.Umbler/              # Projeto principal
    │   ├── Controllers/
    │   │   ├── DomainController.cs  # ⚠️ Controller principal com problemas
    │   │   └── HomeController.cs
    │   ├── Models/
    │   │   ├── DatabaseContext.cs   # DbContext + Entidade Domain
    │   │   └── ErrorViewModel.cs
    │   ├── Views/
    │   │   ├── Home/
    │   │   │   └── Index.cshtml     # ⚠️ Interface sem validação/formatação
    │   │   └── Shared/
    │   ├── src/js/
    │   │   └── app.js               # ⚠️ JavaScript vanilla
    │   ├── Migrations/
    │   ├── Startup.cs
    │   ├── Program.cs
    │   └── webpack.config.js
    └── Desafio.Umbler.Test/         # Projeto de testes
        └── ControllersTests.cs      # ⚠️ Cobertura baixa
```

### **Stack Tecnológica**

#### Backend
- **Framework:** ASP.NET Core 6.0
- **Banco de Dados:** MySQL 8.0.27 (via Pomelo.EntityFrameworkCore.MySql)
- **ORM:** Entity Framework Core 6.0.3
- **Pacotes Principais:**
  - `DnsClient` v1.6.0 - Consultas DNS
  - `WhoisClient.NET` v3.0.1 - Consultas WHOIS
  - `Pomelo.EntityFrameworkCore.MySql` v6.0.1

#### Frontend
- **Build:** Webpack 3.8.1
- **Transpilação:** Babel (ES2015, ES7)
- **Framework:** JavaScript Vanilla (sem framework)
- **Bibliotecas:** Bootstrap, jQuery, jQuery Validation

#### Testes
- **Framework:** MSTest v2.2.8
- **Mocking:** Moq v4.17.2
- **Banco Teste:** Entity Framework InMemory

---

## ⚠️ Problemas Identificados

### 🔴 **CRÍTICO - Backend: DomainController**

#### 1. **Alta Complexidade Ciclomática**
O controller possui múltiplas responsabilidades:
- Busca no banco de dados
- Consultas DNS externas
- Consultas WHOIS externas
- Lógica de negócio (validação TTL)
- Persistência de dados
- Montagem de resposta

**Localização:** `src/Desafio.Umbler/Controllers/DomainController.cs:22-74`

**Problema:** Viola o princípio da Responsabilidade Única (SRP). Dificulta manutenção e testes.

#### 2. **Ausência de Validação**
- Não valida se `domainName` é um domínio válido
- Permite entrada de valores inválidos (ex: "teste", "123", "sem-extensão")
- Resulta em exceções não tratadas (erro 500)

**Exemplo de requisição que quebra:**
```
GET /api/domain/teste
GET /api/domain/123
GET /api/domain/
```

#### 3. **Código Duplicado**
O bloco de código para consultar DNS/WHOIS está duplicado:
- Primeiro bloco: linhas 29-47 (quando domínio não existe)
- Segundo bloco: linhas 54-69 (quando TTL expirou)

**Impacto:** Dificulta manutenção, aumenta risco de bugs.

#### 4. **Retorno de Entidade Completa**
O controller retorna a entidade `Domain` diretamente, expondo propriedades desnecessárias:
- `Id` (informação interna)
- `Ttl` (informação técnica)
- `UpdatedAt` (informação técnica)
- `WhoIs` (dados brutos, muito grandes)

**Solução:** Criar DTO/ViewModel retornando apenas:
- `Name`
- `Ip`
- `HostedAt`
- (Opcionalmente: NameServers formatados)

#### 5. **Dependências Acopladas (Não Mockáveis)**
- `WhoisClient.QueryAsync()` - Classe estática, não pode ser mockada
- `new LookupClient()` - Instanciado diretamente no método

**Impacto:** Impossível testar unitariamente sem fazer chamadas reais a serviços externos.

**Evidência no teste:** `ControllersTests.cs:103-129` - Teste tentando mockar `ILookupClient` mas não consegue mockar `WhoisClient`.

#### 6. **Lógica de TTL Incorreta**
```csharp
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
```

**Problema:** 
- `Ttl` é armazenado em **segundos** (vem do DNS)
- A comparação usa `TotalMinutes` (minutos)
- Isso faz com que o TTL seja considerado expirado muito antes do necessário

**Exemplo:**
- TTL = 3600 segundos (1 hora)
- Após 60 minutos: `60 minutos > 3600` = false ✅
- Mas após 61 minutos: `61 minutos > 3600` = false (deveria ser true!)

**Correção sugerida:**
```csharp
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalSeconds > domain.Ttl)
```

---

### 🟡 **MÉDIO - Frontend: app.js**

#### 1. **Ausência de Validação**
- Não valida formato de domínio antes de enviar requisição
- Permite submissão de campos vazios ou valores inválidos
- Não verifica se o domínio tem extensão válida

**Localização:** `src/Desafio.Umbler/src/js/app.js:48-53`

#### 2. **Exibição Não Formatada**
- Utiliza `JSON.stringify(response, null, 4)` para exibir resultados
- Mostra dados técnicos brutos (Id, Ttl, UpdatedAt, WhoIs completo)
- Não formata IPs, datas, ou organiza informações

**Localização:** `src/Desafio.Umbler/src/js/app.js:51`

#### 3. **Tratamento de Erros Inadequado**
- Não captura exceções da requisição
- Não exibe mensagens de erro amigáveis ao usuário
- Não trata casos de domínio não encontrado ou erros de rede

#### 4. **Uso de JavaScript Vanilla**
- Apesar de ter Webpack e Babel configurados, usa JS vanilla
- Não aproveita frameworks modernos (React, Blazor, Vue)
- Código menos organizado e mais difícil de manter

---

### 🟡 **MÉDIO - Testes Unitários**

#### 1. **Cobertura Muito Baixa**
Apenas 3 testes para o `DomainController`:
- `Domain_In_Database()` - Testa busca quando existe no banco
- `Domain_Not_In_Database()` - Testa criação quando não existe
- `Domain_Moking_LookupClient()` - Tentativa de teste com mock (incompleto)

**Faltam testes para:**
- Validação de entrada
- Lógica de TTL
- Tratamento de erros
- Casos de exceção (DNS falha, WHOIS falha)
- Formatação de resposta

#### 2. **Teste Comentado (OBRIGATÓRIO)**
O teste `Domain_Moking_WhoisClient()` está completamente comentado e **deve passar obrigatoriamente**.

**Localização:** `src/Desafio.Umbler.Test/ControllersTests.cs:132-158`

**Problema:** Não é possível implementar porque `WhoisClient` é estático e não mockável.

**Solução:** Criar wrapper/interface para `WhoisClient` antes de implementar o teste.

#### 3. **Dependências Externas em Testes**
Os testes fazem chamadas reais a serviços externos quando o domínio não está em cache, tornando-os:
- Lentos (esperam respostas de rede)
- Instáveis (podem falhar por problemas de rede)
- Dependentes de serviços externos

---

## 📊 Métricas e Observações

| Aspecto | Status | Observações |
|---------|--------|-------------|
| **Estrutura de Pastas** | ✅ Boa | Organização MVC padrão do ASP.NET Core |
| **Separação de Responsabilidades** | ❌ Ruim | Lógica de negócio misturada com controller |
| **Validação Backend** | ❌ Ausente | Permite requisições inválidas |
| **Validação Frontend** | ❌ Ausente | Permite submissão de dados inválidos |
| **Tratamento de Erros** | ❌ Inadequado | Exceções não tratadas, retorna 500 |
| **Testabilidade** | ❌ Baixa | Dependências estáticas não mockáveis |
| **Cobertura de Testes** | ❌ Baixa | Apenas 3 testes básicos |
| **DTOs/ViewModels** | ❌ Ausente | Retorna entidades diretamente |
| **Código Duplicado** | ❌ Presente | Bloco de consulta DNS/WHOIS duplicado |
| **Lógica de TTL** | ❌ Incorreta | Compara minutos com segundos |
| **Documentação** | ✅ Boa | README claro com objetivos definidos |

---

## 🎯 Recomendações por Prioridade

### 🔴 **PRIORIDADE ALTA**

#### 1. **Refatorar Arquitetura em Camadas**
**Objetivo:** Separar responsabilidades

**Sugestão de estrutura:**
```
src/Desafio.Umbler/
├── Controllers/
│   └── DomainController.cs          # Apenas recebe requisição e retorna resposta
├── Services/
│   ├── IDomainService.cs
│   └── DomainService.cs             # Lógica de negócio
├── Repositories/
│   ├── IDomainRepository.cs
│   └── DomainRepository.cs          # Acesso a dados
├── Services/
│   ├── IDnsService.cs               # Abstração para DNS
│   ├── DnsService.cs
│   ├── IWhoisService.cs             # Abstração para WHOIS
│   └── WhoisService.cs
└── ViewModels/
    └── DomainViewModel.cs           # DTO para resposta
```

**Benefícios:**
- Controller fica simples (apenas orquestração)
- Lógica testável independentemente
- Fácil de manter e estender

#### 2. **Criar Interfaces para DNS e WHOIS**
**Objetivo:** Permitir mocking nos testes

**Exemplo:**
```csharp
public interface IWhoisService
{
    Task<WhoisResponse> QueryAsync(string domain);
}

public interface IDnsService
{
    Task<DnsResponse> QueryAsync(string domain);
}
```

**Benefícios:**
- Testes unitários isolados
- Possibilidade de mockar dependências externas
- Fácil trocar implementação (ex: para testes)

#### 3. **Criar DTOs/ViewModels**
**Objetivo:** Não expor entidade completa

**Exemplo:**
```csharp
public class DomainViewModel
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public string HostedAt { get; set; }
    public List<string> NameServers { get; set; }
}
```

**Benefícios:**
- API mais limpa
- Não expõe dados internos
- Controle sobre o que é retornado

#### 4. **Implementar Validação no Backend**
**Objetivo:** Validar entrada antes de processar

**Opções:**
- **Data Annotations:** Criar modelo de validação
- **FluentValidation:** Biblioteca dedicada
- **Validação Manual:** Regex para formato de domínio

**Validações necessárias:**
- Domínio não vazio
- Formato válido de domínio (ex: `exemplo.com`, `sub.exemplo.com`)
- Extensão válida (TLD)

#### 5. **Corrigir Lógica de TTL**
**Correção:**
```csharp
// ❌ ERRADO (atual)
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)

// ✅ CORRETO
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalSeconds > domain.Ttl)
```

**Localização:** `DomainController.cs:52`

---

### 🟡 **PRIORIDADE MÉDIA**

#### 6. **Implementar Validação no Frontend**
**Validações sugeridas:**
- Campo não vazio
- Formato de domínio válido (regex)
- Feedback visual (campos com erro em vermelho)

#### 7. **Formatar Exibição de Resultados**
**Sugestões:**
- Cards/Componentes visuais para cada informação
- Formatação de IPs
- Formatação de datas (se necessário)
- Exibir Name Servers de forma organizada
- Ocultar dados técnicos (Id, Ttl, UpdatedAt, WhoIs raw)

#### 8. **Melhorar Tratamento de Erros**
**No Frontend:**
- Try/catch nas requisições
- Mensagens de erro amigáveis
- Feedback visual (toast, alert, etc.)

**No Backend:**
- Tratamento de exceções específicas (DNS não encontrado, WHOIS falhou)
- Retornar códigos HTTP apropriados (400, 404, 500)
- Mensagens de erro descritivas

#### 9. **Implementar Teste Comentado**
**Teste obrigatório:** `Domain_Moking_WhoisClient()`

**Pré-requisito:** Criar interface `IWhoisService` primeiro.

#### 10. **Aumentar Cobertura de Testes**
**Testes sugeridos:**
- Validação de entrada inválida
- Domínio não encontrado no DNS
- Erro na consulta WHOIS
- TTL expirado (atualização de dados)
- TTL não expirado (retorno do cache)
- Persistência no banco
- Mapeamento Domain → DomainViewModel

---

### 🟢 **PRIORIDADE BAIXA**

#### 11. **Considerar Framework Frontend Moderno**
**Opções:**
- **React:** Já tem Webpack/Babel configurado
- **Blazor:** Integração nativa com .NET
- **Vue.js:** Alternativa leve

#### 12. **Melhorias de UX**
- Loading state durante requisição
- Desabilitar botão durante busca
- Feedback visual de sucesso/erro
- Histórico de consultas recentes

#### 13. **Logging**
- Adicionar logging estruturado
- Registrar consultas DNS/WHOIS
- Log de erros para debug

---

## 📝 Checklist de Implementação

Use este checklist para acompanhar o progresso das melhorias:

### Backend
- [ ] Criar estrutura de camadas (Services, Repositories)
- [ ] Criar interface `IWhoisService` e implementação
- [ ] Criar interface `IDnsService` e implementação
- [ ] Criar `DomainViewModel` (DTO)
- [ ] Refatorar `DomainController` para usar serviços
- [ ] Implementar validação de domínio no controller
- [ ] Corrigir lógica de TTL (segundos ao invés de minutos)
- [ ] Remover código duplicado (extrair método)
- [ ] Adicionar tratamento de erros adequado
- [ ] Configurar injeção de dependência no `Startup.cs`

### Frontend
- [ ] Implementar validação de formato de domínio
- [ ] Criar componente/formatação para exibir resultados
- [ ] Adicionar tratamento de erros (try/catch)
- [ ] Adicionar feedback visual (loading, erro, sucesso)
- [ ] Ocultar dados técnicos na exibição

### Testes
- [ ] Implementar teste `Domain_Moking_WhoisClient()` (obrigatório)
- [ ] Criar testes para validação de entrada
- [ ] Criar testes para lógica de TTL
- [ ] Criar testes para tratamento de erros
- [ ] Criar testes para mapeamento Domain → DomainViewModel
- [ ] Aumentar cobertura para pelo menos 70%

### Documentação
- [ ] Atualizar README com descrição das mudanças
- [ ] Documentar novas interfaces e serviços
- [ ] Adicionar exemplos de uso da API

---

## 🔍 Pontos de Atenção Específicos

### 1. **Teste Obrigatório (Domain_Moking_WhoisClient)**
**Status:** ❌ Não implementado (comentado)

**Requisito:** Este teste DEVE passar obrigatoriamente.

**Desafio:** `WhoisClient` é uma classe estática, não pode ser mockada diretamente.

**Solução:** Criar wrapper/interface:
```csharp
public interface IWhoisService
{
    Task<WhoisResponse> QueryAsync(string query);
}

public class WhoisService : IWhoisService
{
    public async Task<WhoisResponse> QueryAsync(string query)
    {
        return await WhoisClient.QueryAsync(query);
    }
}
```

**Depois:** Injetar `IWhoisService` no controller e mockar nos testes.

### 2. **Lógica de TTL**
**Localização:** `DomainController.cs:52`

**Problema Atual:**
```csharp
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
```

**Correção Necessária:**
```csharp
if (DateTime.Now.Subtract(domain.UpdatedAt).TotalSeconds > domain.Ttl)
```

**Por quê:** TTL vem em segundos do DNS, mas o código compara com minutos.

### 3. **Código Duplicado**
**Localização:** 
- Primeira ocorrência: `DomainController.cs:29-47`
- Segunda ocorrência: `DomainController.cs:54-69`

**Solução:** Extrair para método privado ou melhor ainda, mover para service:
```csharp
private async Task<Domain> QueryDomainInfoAsync(string domainName)
{
    // Lógica de consulta DNS/WHOIS
}
```

---

## 📚 Referências e Padrões

### Arquitetura em Camadas
- **Controller:** Recebe requisições HTTP, valida entrada, retorna resposta
- **Service:** Contém lógica de negócio, orquestra chamadas
- **Repository:** Acesso a dados (banco de dados)
- **Model/Entity:** Representação dos dados no banco
- **ViewModel/DTO:** Representação dos dados para API/Frontend

### Princípios SOLID
- **S**ingle Responsibility: Cada classe uma responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Interfaces devem ser substituíveis
- **I**nterface Segregation: Interfaces específicas
- **D**ependency Inversion: Depender de abstrações, não implementações

### Padrões de Design Úteis
- **Repository Pattern:** Para acesso a dados
- **Service Layer:** Para lógica de negócio
- **DTO Pattern:** Para transferência de dados
- **Dependency Injection:** Para desacoplamento

---

## 🚀 Próximos Passos Sugeridos

1. **Fase 1: Arquitetura**
   - Criar interfaces (IWhoisService, IDnsService)
   - Criar serviços (WhoisService, DnsService)
   - Criar DomainService para orquestração
   - Configurar DI no Startup

2. **Fase 2: Refatoração**
   - Refatorar DomainController
   - Criar DomainViewModel
   - Corrigir lógica de TTL
   - Remover código duplicado

3. **Fase 3: Validação e Erros**
   - Implementar validação backend
   - Adicionar tratamento de erros
   - Implementar validação frontend
   - Melhorar feedback visual

4. **Fase 4: Testes**
   - Implementar teste obrigatório
   - Criar testes adicionais
   - Aumentar cobertura

5. **Fase 5: Frontend**
   - Formatar exibição de resultados
   - Melhorar UX
   - (Opcional) Migrar para framework moderno

---

## 📌 Observações Finais

- Este é um projeto de **desafio técnico**, focado em demonstrar boas práticas
- O README já documenta claramente os problemas a resolver
- **Não há "pegadinhas"** - o objetivo é melhorar código existente
- Há **dicas nos comentários dos testes** - leia atentamente
- O teste comentado **deve passar obrigatoriamente**

**Boa sorte! 🚀**

