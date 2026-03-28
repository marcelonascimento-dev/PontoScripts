# PontoScripts

Aplicação web interna para gerenciamento de scripts SQL do produto **Ponto/Frequência** (sistema de RH/ponto eletrônico da LG).

## Problema

O desenvolvimento do Ponto/Frequência envolve dois arquivos SQL monolíticos editados por toda a equipe:

- **`Globalizacao.sql`** (~41 mil linhas) — traduções da aplicação em pt-BR, en-US e es-ES
- **`ScriptAlteracaoBanco.sql`** (~9 mil linhas) — alterações de schema do banco de dados

Edição simultânea desses arquivos causa **conflitos de merge** e **corrupção de encoding**. O PontoScripts resolve isso permitindo que cada desenvolvedor cadastre seus itens individualmente, e a equipe de operações gere os arquivos SQL consolidados por versão.

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
