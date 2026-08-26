# dotnet-domain-lab

Laboratório .NET para consulta de informações DNS e WHOIS de domínios.

Parte da coleção [novaxis/Labs](https://github.com/lbulegon) — projeto independente para experimentação com ASP.NET Core, Entity Framework, integrações externas e arquitetura em camadas.

## O que faz

Recebe um domínio (ex.: `example.com`) e retorna:

- Name servers (registros NS)
- IP do registro A
- Organização de hospedagem (via WHOIS do IP)
- Cache em MySQL respeitando TTL do registro DNS

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 6 / C# |
| Web | ASP.NET Core MVC + Blazor Server |
| Persistência | EF Core 6 + Pomelo (MySQL) |
| DNS / WHOIS | DnsClient, WhoisClient.NET |
| Logging | Serilog (console + arquivo) |
| Testes | MSTest, Moq, EF InMemory |

## Pré-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MySQL](https://www.mysql.com/) (local ou remoto)
- Visual Studio ou VS Code

## Configuração

1. Ajuste a connection string em `src/Desafio.Umbler/appsettings.Development.json` (não versionada) ou use User Secrets:

```bash
cd src/Desafio.Umbler
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=domain_lab;Uid=root;Pwd=SUA_SENHA;"
```

2. Aplique as migrations:

```bash
dotnet tool update --global dotnet-ef
dotnet ef database update --project src/Desafio.Umbler
```

3. Execute a aplicação:

```bash
dotnet run --project src/Desafio.Umbler
```

## Estrutura

```
dotnet-domain-lab/
├── src/
│   ├── Desafio.Umbler/          # API + UI (MVC/Blazor)
│   └── Desafio.Umbler.Test/     # Testes unitários
├── docs/                        # Notas de arquitetura e melhorias
└── Desafio.Umbler.sln
```

## Documentação adicional

- `docs/MELHORIAS_IMPLEMENTADAS.md` — histórico de refatorações
- `docs/ANALISE_IMPLEMENTACAO_VS_REQUISITOS.md` — análise técnica
- `docs/TAREFAS_SOLICITADAS.md` — backlog de melhorias

## Licença

Uso pessoal / laboratório. Código baseado em exercício de consulta DNS; evoluído como sandbox de aprendizado .NET.
