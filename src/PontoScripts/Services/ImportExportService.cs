using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PontoScripts.Data;
using PontoScripts.Models;

namespace PontoScripts.Services;

public class ImportExportService(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<byte[]> ExportarGlobalizacoesAsync()
    {
        var entries = await db.GlobalizacaoEntries
            .Select(g => new GlobalizacaoExportDto
            {
                Tipo = g.Tipo,
                Mensagem = g.Mensagem,
                TraducaoPtBR = g.TraducaoPtBR,
                TraducaoEnUS = g.TraducaoEnUS,
                TraducaoEsES = g.TraducaoEsES,
                AtributoAdicional = g.AtributoAdicional,
                DataCriacao = g.DataCriacao
            })
            .ToListAsync();

        return JsonSerializer.SerializeToUtf8Bytes(entries, JsonOptions);
    }

    public async Task<byte[]> ExportarScriptsAsync()
    {
        var entries = await db.ScriptAlteracaoEntries
            .Select(s => new ScriptExportDto
            {
                NumeroOS = s.NumeroOS,
                Branch = s.Branch,
                Descricao = s.Descricao,
                ScriptSql = s.ScriptSql,
                CriadoPor = s.CriadoPor,
                OrdemExecucao = s.OrdemExecucao,
                DataCriacao = s.DataCriacao
            })
            .ToListAsync();

        return JsonSerializer.SerializeToUtf8Bytes(entries, JsonOptions);
    }

    public async Task<ImportResult> ImportarGlobalizacoesAsync(Stream stream)
    {
        var dtos = await JsonSerializer.DeserializeAsync<List<GlobalizacaoExportDto>>(stream, JsonOptions);
        if (dtos is null || dtos.Count == 0)
            return new ImportResult(0, 0, ["Arquivo vazio ou formato inválido."]);

        int importados = 0, ignorados = 0;
        var erros = new List<string>();

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo) || string.IsNullOrWhiteSpace(dto.Mensagem))
            {
                erros.Add($"Entrada ignorada: Tipo e Mensagem são obrigatórios.");
                ignorados++;
                continue;
            }

            var existe = await db.GlobalizacaoEntries.AnyAsync(g => g.Tipo == dto.Tipo && g.Mensagem == dto.Mensagem);
            if (existe)
            {
                ignorados++;
                continue;
            }

            db.GlobalizacaoEntries.Add(new GlobalizacaoEntry
            {
                Tipo = dto.Tipo,
                Mensagem = dto.Mensagem,
                TraducaoPtBR = dto.TraducaoPtBR,
                TraducaoEnUS = dto.TraducaoEnUS,
                TraducaoEsES = dto.TraducaoEsES,
                AtributoAdicional = dto.AtributoAdicional ?? "N",
                DataCriacao = dto.DataCriacao ?? DateTime.Now
            });
            importados++;
        }

        await db.SaveChangesAsync();
        return new ImportResult(importados, ignorados, erros);
    }

    public async Task<ImportResult> ImportarScriptsAsync(Stream stream)
    {
        var dtos = await JsonSerializer.DeserializeAsync<List<ScriptExportDto>>(stream, JsonOptions);
        if (dtos is null || dtos.Count == 0)
            return new ImportResult(0, 0, ["Arquivo vazio ou formato inválido."]);

        int importados = 0, ignorados = 0;
        var erros = new List<string>();

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Descricao) || string.IsNullOrWhiteSpace(dto.ScriptSql))
            {
                erros.Add($"Script ignorado: Descrição e ScriptSql são obrigatórios.");
                ignorados++;
                continue;
            }

            db.ScriptAlteracaoEntries.Add(new ScriptAlteracaoEntry
            {
                NumeroOS = dto.NumeroOS,
                Branch = dto.Branch,
                Descricao = dto.Descricao,
                ScriptSql = dto.ScriptSql,
                CriadoPor = dto.CriadoPor ?? "Importado",
                OrdemExecucao = dto.OrdemExecucao,
                DataCriacao = dto.DataCriacao ?? DateTime.Now
            });
            importados++;
        }

        await db.SaveChangesAsync();
        return new ImportResult(importados, ignorados, erros);
    }
}

public record ImportResult(int Importados, int Ignorados, List<string> Erros);

public class GlobalizacaoExportDto
{
    public string Tipo { get; set; } = "";
    public string Mensagem { get; set; } = "";
    public string TraducaoPtBR { get; set; } = "";
    public string TraducaoEnUS { get; set; } = "";
    public string TraducaoEsES { get; set; } = "";
    public string? AtributoAdicional { get; set; }
    public DateTime? DataCriacao { get; set; }
}

public class ScriptExportDto
{
    public string? NumeroOS { get; set; }
    public string? Branch { get; set; }
    public string Descricao { get; set; } = "";
    public string ScriptSql { get; set; } = "";
    public string? CriadoPor { get; set; }
    public int OrdemExecucao { get; set; }
    public DateTime? DataCriacao { get; set; }
}
