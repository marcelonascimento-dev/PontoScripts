# PontoScripts

## O que é este projeto

PontoScripts é uma aplicação web interna criada para resolver dois problemas críticos no fluxo de desenvolvimento do produto **Ponto/Frequência** (sistema de RH/ponto eletrônico da LG):

1. **Erros de encoding** no arquivo monolítico `Globalizacao.sql` (~41 mil linhas), que contém traduções da aplicação em 3 idiomas (pt-BR, en-US, es-ES)
2. **Perda de scripts SQL** durante merges no arquivo monolítico `ScriptAlteracaoBanco.sql` (~9 mil linhas), que contém alterações de schema do banco de dados

Em vez de todos os desenvolvedores editarem esses dois arquivos gigantes (causando conflitos de merge e corrupção de encoding), cada desenvolvedor cadastra seus itens individualmente nesta ferramenta. A equipe de operações então gera os arquivos SQL consolidados por versão, prontos para deploy.

## Stack Técnica

- **ASP.NET Core 10 Blazor Server** (interatividade server-side, C# único)
- **Entity Framework Core + SQLite** (banco local, zero config)
- **Bootstrap 5 + Bootstrap Icons** (via CDN)

## Estrutura do Projeto

```
src/PontoScripts/
├── Models/          → Entidades: GlobalizacaoEntry, ScriptAlteracaoEntry, Versao, VersaoGlobalizacao, VersaoScript
├── Data/            → AppDbContext + Migrations (EF Core SQLite)
├── Services/        → Lógica de negócio e geração de SQL
├── Components/      → Páginas Blazor (Layout, Home, Globalizacao, Scripts, Versoes)
└── Program.cs       → Configuração, DI, endpoints de download
```

## Duas funcionalidades independentes

### 1. Globalização
- Desenvolvedores cadastram **entradas de globalização** com traduções em 3 idiomas lado a lado
- Usa stored procedure `SP_PONTO_GLOBALIZACAO_INSERIR_SE_NAO_EXISTIR` na geração do SQL
- Validação de encoding anti-mojibake em todas as entradas

### 2. Scripts de Alteração de Banco
- Desenvolvedores cadastram **scripts SQL** diretamente
- Campos: OS (texto livre, opcional), Branch, Descrição, Criado Por, Script SQL
- O dev cola o bloco completo do script (IF NOT EXISTS, BEGIN/END, etc.) — responsabilidade é do dev
- Validação básica de sintaxe: BEGIN/END, parênteses, aspas, comentários de bloco

### Versionamento
- Operações cria uma **Versão** selecionando Globalizações pendentes e Scripts pendentes
- Preview do SQL gerado em duas abas (Globalizacao.sql e ScriptAlteracaoBanco.sql)
- Download dos arquivos em UTF-8 com BOM
- Status da versão: Aberta → Gerada → Liberada

## Endpoints de Download
- `GET /api/versao/{id}/download/globalizacao` → Globalizacao.sql
- `GET /api/versao/{id}/download/scriptalteracao` → ScriptAlteracaoBanco.sql

## Como Rodar
```bash
dotnet run --project src/PontoScripts --urls "http://localhost:5050"
```
O banco SQLite é criado e migrado automaticamente na primeira execução.

## Convenções
- Nomes de entidades e serviços em português
- UI em português brasileiro
- Todos os arquivos SQL gerados devem ter encoding UTF-8 com BOM (3 bytes: 0xEF, 0xBB, 0xBF)
- Scripts de alteração são emitidos exatamente como o dev cadastrou, sem wrapping automático
