# PontoScripts

Aplicação web interna para gerenciamento centralizado de scripts SQL e traduções (globalização), com geração de arquivos versionados prontos para deploy.

## Funcionalidades

### Globalização

- Cadastro de entradas de globalização com traduções em 3 idiomas lado a lado
- Validação de encoding anti-mojibake
- Geração de SQL usando `SP_PONTO_GLOBALIZACAO_INSERIR_SE_NAO_EXISTIR`

### Scripts de Alteração de Banco

- Cadastro de scripts SQL com campos: OS, Branch, Descrição, Criado Por e Script SQL
- Validação básica de sintaxe (BEGIN/END, parênteses, aspas, comentários de bloco)

### Versionamento

- Criação de versões selecionando entradas pendentes de Globalização e Scripts
- Preview do SQL gerado em duas abas
- Download dos arquivos em UTF-8 com BOM
- Status da versão: **Aberta** → **Gerada** → **Liberada**

## Stack Técnica

- **ASP.NET Core 10** — Blazor Server (interatividade server-side)
- **Entity Framework Core** + **SQLite** (banco local, zero config)
- **Bootstrap 5** + **Bootstrap Icons** (via CDN)

## Como Rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Executar

```bash
dotnet run --project src/PontoScripts --urls "http://localhost:5050"
```

O banco SQLite é criado e migrado automaticamente na primeira execução.

## Estrutura do Projeto

```
src/PontoScripts/
├── Models/          → Entidades (GlobalizacaoEntry, ScriptAlteracaoEntry, Versao, etc.)
├── Data/            → AppDbContext + Migrations (EF Core SQLite)
├── Services/        → Lógica de negócio e geração de SQL
├── Components/      → Páginas Blazor (Layout, Home, Globalizacao, Scripts, Versoes)
└── Program.cs       → Configuração, DI, endpoints de download
```

## API

| Endpoint | Descrição |
|---|---|
| `GET /api/versao/{id}/download/globalizacao` | Download do `Globalizacao.sql` |
| `GET /api/versao/{id}/download/scriptalteracao` | Download do `ScriptAlteracaoBanco.sql` |
