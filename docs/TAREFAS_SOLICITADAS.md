# 📋 Tarefas e Melhorias Solicitadas no Teste

Este documento lista **apenas** as melhorias e tarefas explicitamente solicitadas no README do desafio.

---

## 🎯 Objetivos do Teste

O projeto já funciona, mas precisa de melhorias em vários pontos:

---

## 🎨 Frontend

### 1. Formatação de Dados Retornados
**Problema:** Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.

**O que fazer:**
- Apresentar os dados de forma legível ao invés de JSON.stringify
- Formatar visualmente as informações do domínio
- Organizar Name servers, IP e empresa hospedadora

**Status:** 📅 Planejado

---

### 2. Validação no Frontend
**Problema:** Não há validação no frontend permitindo que seja submetida uma requisição inválida para o servidor (por exemplo, um domínio sem extensão).

**O que fazer:**
- Implementar validação de formato de domínio antes de enviar requisição
- Validar se o domínio tem extensão válida
- Impedir submissão de dados inválidos
- Fornecer feedback visual ao usuário

**Status:** 📅 Planejado

---

-- TODO
### 3. Framework Moderno (Opcional)
**Problema:** Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack.

**O que fazer:**
- Utilizar algum framework mais moderno como ReactJs ou Blazor
- Aproveitar a configuração do webpack já existente

**Observação:** Esta é uma sugestão, não obrigatória. O ideal seria usar um framework moderno.

**Status:** 📅 Planejado (Opcional)

---

## ⚙️ Backend

### 4. Validação no Backend
**Problema:** Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).

**O que fazer:**
- Implementar validação de formato de domínio no backend
- Validar entrada antes de processar
- Retornar erro apropriado (400 Bad Request) ao invés de 500
- Tratar exceções adequadamente

**Status:** 🚧 Parcialmente Implementado (validação básica existe, mas precisa ser melhorada)

---

### 5. Arquitetura em Camadas
**Problema:** A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.

**O que fazer:**
- Separar responsabilidades
- Mover lógica de negócio do controller para uma camada de serviços
- Implementar Repository Pattern para acesso a dados
- Reduzir complexidade do controller

**Status:** 🚧 Em Andamento
- ✅ Interfaces IWhoisService e IDnsService criadas
- ✅ Implementações WhoisService e DnsService criadas
- ✅ Repository Pattern implementado (IDomainRepository, DomainRepository)
- ✅ DomainViewModel criado
- ⏳ DomainService precisa ser criado
- ⏳ Controller precisa ser refatorado

---

### 6. ViewModel (DTO)
**Problema:** O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente.

**O que fazer:**
- Criar uma ViewModel (DTO) para retornar apenas os dados necessários
- Retornar apenas: Name, Ip, HostedAt
- Não expor: Id, Ttl, UpdatedAt, WhoIs (dados técnicos/internos)

**Status:** ✅ ViewModel Criado, ⏳ Controller precisa retornar ViewModel

**ViewModel Criado:**
```csharp
public class DomainViewModel
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public string HostedAt { get; set; }
}
```

---

## 🧪 Testes

### 7. Mockar Consultas Whois e Dns
**Problema:** O DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura. O banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não.

**O que fazer:**
- Criar interfaces para WhoisClient e DnsClient (ou wrapper)
- Permitir mockar essas dependências nos testes
- Tornar o DomainController testável

**Status:** 🚧 Em Andamento
- ✅ Interface IWhoisService criada
- ✅ Interface IDnsService criada
- ⏳ Controller precisa usar essas interfaces
- ⏳ Testes precisam ser atualizados

---

### 8. Teste Obrigatório (Domain_Moking_WhoisClient)
**Requisito:** Há um teste unitário que está comentado, que **obrigatoriamente tem que passar**.

**Arquivo:** `src/Desafio.Umbler.Test/ControllersTests.cs`
**Método:** `Domain_Moking_WhoisClient()` (linhas 132-158)

**O que fazer:**
- Implementar o teste que está comentado
- Garantir que o teste passa
- Este teste valida que é possível mockar o WhoisClient

**Status:** 📅 Planejado (Aguardando implementação das interfaces no controller)

---

### 9. Aumentar Cobertura de Testes
**Problema:** A cobertura de testes unitários está muito baixa.

**O que fazer:**
- Aumentar cobertura de testes
- Criar mais testes unitários
- Testar diferentes cenários

**Observação:** Criar mais testes é um **diferencial**, não obrigatório, mas muito desejável.

**Status:** 📅 Planejado

---

## 📝 Entrega

### 10. Documentar Modificações
**Requisito:** Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

**O que fazer:**
- Atualizar a seção "Modificações" do README
- Descrever o objetivo das mudanças realizadas
- Explicar o motivo de cada melhoria implementada

**Status:** 📅 A fazer após completar as melhorias

---

## 📊 Resumo de Status

| Categoria | Tarefa | Status | Prioridade |
|-----------|--------|--------|------------|
| Frontend | Formatação de dados | 📅 Planejado | Alta |
| Frontend | Validação | 📅 Planejado | Alta |
| Frontend | Framework moderno | 📅 Planejado | Opcional |
| Backend | Validação | 🚧 Parcial | Alta |
| Backend | Arquitetura em camadas | 🚧 Em Andamento | Alta |
| Backend | ViewModel/DTO | ✅ Criado, ⏳ Falta usar | Alta |
| Testes | Mockar Whois/Dns | 🚧 Em Andamento | Alta |
| Testes | Teste obrigatório | 📅 Planejado | **Obrigatório** |
| Testes | Mais testes | 📅 Planejado | Diferencial |
| Entrega | Documentar mudanças | 📅 Planejado | Obrigatório |

---

## 🎯 Checklist das Tarefas Obrigatórias

### Frontend
- [ ] Formatar dados retornados de forma legível
- [ ] Implementar validação de formato de domínio no frontend
- [ ] (Opcional) Migrar para framework moderno

### Backend
- [x] Implementar validação básica (melhorar)
- [ ] Completar arquitetura em camadas
  - [x] Interfaces IWhoisService e IDnsService
  - [x] Implementações WhoisService e DnsService
  - [x] Repository Pattern
  - [x] DomainViewModel criado
  - [ ] DomainService criado
  - [ ] Refatorar Controller
- [ ] Controller retornar ViewModel ao invés de entidade

### Testes
- [ ] Implementar teste obrigatório `Domain_Moking_WhoisClient()` (DEVE PASSAR)
- [ ] Controller usar interfaces para permitir mock
- [ ] (Diferencial) Criar mais testes unitários

### Entrega
- [ ] Documentar modificações no README

---

## 📌 Observações Importantes

1. **Teste Obrigatório:** O teste `Domain_Moking_WhoisClient()` **DEVE PASSAR**. Este é um requisito obrigatório.

2. **Dicas nos Testes:** O README menciona que há dicas textuais deixadas nos testes unitários. Leia atentamente os comentários nos testes.

3. **Não há "pegadinhas":** O teste é pensado para ser simples, não há pegadinhas.

4. **Diferencial:** Criar mais testes é um diferencial, mas não obrigatório.

---

**Última Atualização:** 17/12/2025


