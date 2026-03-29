-- ============================================================
-- PontoScripts - Script de criação do banco de dados
-- Compatível com SQL Server / Azure SQL Database
-- Execução idempotente (seguro executar mais de uma vez)
-- ============================================================

-- ============================================================
-- 1. TABELAS PRINCIPAIS
-- ============================================================

-- GlobalizacaoEntries
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GlobalizacaoEntries]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[GlobalizacaoEntries] (
        [Id]                  INT            IDENTITY(1,1) NOT NULL,
        [Tipo]                NVARCHAR(450)  NOT NULL,
        [Mensagem]            NVARCHAR(450)  NOT NULL,
        [TraducaoPtBR]        NVARCHAR(MAX)  NOT NULL,
        [TraducaoEnUS]        NVARCHAR(MAX)  NOT NULL,
        [TraducaoEsES]        NVARCHAR(MAX)  NOT NULL,
        [AtributoAdicional]   NVARCHAR(200)  NOT NULL,
        [DataCriacao]         DATETIME2      NOT NULL,
        CONSTRAINT [PK_GlobalizacaoEntries] PRIMARY KEY CLUSTERED ([Id])
    )
END
GO

-- ScriptAlteracaoEntries
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ScriptAlteracaoEntries]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[ScriptAlteracaoEntries] (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [NumeroOS]        NVARCHAR(MAX)  NULL,
        [Branch]          NVARCHAR(MAX)  NULL,
        [Descricao]       NVARCHAR(MAX)  NOT NULL,
        [ScriptSql]       NVARCHAR(MAX)  NOT NULL,
        [CriadoPor]       NVARCHAR(MAX)  NOT NULL,
        [OrdemExecucao]   INT            NOT NULL,
        [DataCriacao]     DATETIME2      NOT NULL,
        CONSTRAINT [PK_ScriptAlteracaoEntries] PRIMARY KEY CLUSTERED ([Id])
    )
END
GO

-- Versoes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Versoes]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Versoes] (
        [Id]           INT            IDENTITY(1,1) NOT NULL,
        [Numero]       NVARCHAR(MAX)  NOT NULL,
        [Observacoes]  NVARCHAR(MAX)  NULL,
        [DataCriacao]  DATETIME2      NOT NULL,
        [DataGeracao]  DATETIME2      NULL,
        [Status]       NVARCHAR(MAX)  NOT NULL,
        CONSTRAINT [PK_Versoes] PRIMARY KEY CLUSTERED ([Id])
    )
END
GO

-- ============================================================
-- 2. TABELAS DE RELACIONAMENTO (N:N)
-- ============================================================

-- VersaoGlobalizacoes (Versao <-> GlobalizacaoEntry)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VersaoGlobalizacoes]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[VersaoGlobalizacoes] (
        [VersaoId]              INT NOT NULL,
        [GlobalizacaoEntryId]   INT NOT NULL,
        CONSTRAINT [PK_VersaoGlobalizacoes] PRIMARY KEY CLUSTERED ([VersaoId], [GlobalizacaoEntryId]),
        CONSTRAINT [FK_VersaoGlobalizacoes_Versoes_VersaoId] FOREIGN KEY ([VersaoId])
            REFERENCES [dbo].[Versoes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_VersaoGlobalizacoes_GlobalizacaoEntries_GlobalizacaoEntryId] FOREIGN KEY ([GlobalizacaoEntryId])
            REFERENCES [dbo].[GlobalizacaoEntries] ([Id]) ON DELETE CASCADE
    )
END
GO

-- VersaoScripts (Versao <-> ScriptAlteracaoEntry)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VersaoScripts]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[VersaoScripts] (
        [VersaoId]                  INT NOT NULL,
        [ScriptAlteracaoEntryId]    INT NOT NULL,
        CONSTRAINT [PK_VersaoScripts] PRIMARY KEY CLUSTERED ([VersaoId], [ScriptAlteracaoEntryId]),
        CONSTRAINT [FK_VersaoScripts_Versoes_VersaoId] FOREIGN KEY ([VersaoId])
            REFERENCES [dbo].[Versoes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_VersaoScripts_ScriptAlteracaoEntries_ScriptAlteracaoEntryId] FOREIGN KEY ([ScriptAlteracaoEntryId])
            REFERENCES [dbo].[ScriptAlteracaoEntries] ([Id]) ON DELETE CASCADE
    )
END
GO

-- ============================================================
-- 3. ÍNDICES
-- ============================================================

-- Índice único: Tipo + Mensagem (evita duplicatas de globalização)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GlobalizacaoEntries_Tipo_Mensagem' AND object_id = OBJECT_ID(N'[dbo].[GlobalizacaoEntries]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_GlobalizacaoEntries_Tipo_Mensagem]
    ON [dbo].[GlobalizacaoEntries] ([Tipo], [Mensagem])
END
GO

-- Índice FK: VersaoGlobalizacoes -> GlobalizacaoEntryId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VersaoGlobalizacoes_GlobalizacaoEntryId' AND object_id = OBJECT_ID(N'[dbo].[VersaoGlobalizacoes]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_VersaoGlobalizacoes_GlobalizacaoEntryId]
    ON [dbo].[VersaoGlobalizacoes] ([GlobalizacaoEntryId])
END
GO

-- Índice FK: VersaoScripts -> ScriptAlteracaoEntryId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VersaoScripts_ScriptAlteracaoEntryId' AND object_id = OBJECT_ID(N'[dbo].[VersaoScripts]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_VersaoScripts_ScriptAlteracaoEntryId]
    ON [dbo].[VersaoScripts] ([ScriptAlteracaoEntryId])
END
GO

-- ============================================================
-- 4. TABELA DE CONTROLE DE MIGRATIONS (EF Core)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId]      NVARCHAR(150) NOT NULL,
        [ProductVersion]   NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId])
    )
END
GO

-- Registrar a migration como já aplicada (evita que o EF tente recriar)
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20260328204831_InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260328204831_InitialCreate', '10.0.5')
END
GO

PRINT 'PontoScripts - Base de dados criada com sucesso!'
GO
