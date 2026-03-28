using System.Text;
using PontoScripts.Models;

namespace PontoScripts.Services;

public class SqlGeneratorService
{
    private static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];

    public byte[] GerarGlobalizacao(Versao versao)
    {
        var sb = new StringBuilder();
        sb.AppendLine("--*- encoding: utf-8 -*-");
        sb.AppendLine("-- Arquivo gerado em UTF-8");
        sb.AppendLine($"-- Versão: {versao.Numero}");
        sb.AppendLine($"-- Data de geração: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();

        var globalizacoes = versao.VersaoGlobalizacoes
            .Select(vg => vg.GlobalizacaoEntry)
            .OrderBy(g => g.Tipo)
            .ThenBy(g => g.Mensagem)
            .ToList();

        foreach (var glob in globalizacoes)
        {
            GerarExecGlobalizacao(sb, glob.Mensagem, glob.TraducaoPtBR, glob.Tipo, "pt-BR");
            GerarExecGlobalizacao(sb, glob.Mensagem, glob.TraducaoEnUS, glob.Tipo, "en-US");
            GerarExecGlobalizacao(sb, glob.Mensagem, glob.TraducaoEsES, glob.Tipo, "es-ES");
            sb.AppendLine();
        }

        sb.AppendLine("-- ATENÇÃO : ESTE ARQUIVO ESTÁ NO FORMATO UTF-8 LOGO VERIFIQUE O FORMATO DAS ALTERAÇÕES ANTES DO COMMIT");

        return [.. Utf8Bom, .. Encoding.UTF8.GetBytes(sb.ToString())];
    }

    private static void GerarExecGlobalizacao(StringBuilder sb, string mensagem, string traducao, string tipo, string idioma)
    {
        sb.AppendLine("EXEC DBO.SP_PONTO_GLOBALIZACAO_INSERIR_SE_NAO_EXISTIR");
        sb.AppendLine($"    @Mensagem = N'{EscapeSql(mensagem)}',");
        sb.AppendLine($"    @Traducao = N'{EscapeSql(traducao)}',");
        sb.AppendLine($"    @Tipo = N'{EscapeSql(tipo)}',");
        sb.AppendLine($"    @Idioma = N'{EscapeSql(idioma)}'");
        sb.AppendLine("GO");
    }

    public byte[] GerarScriptAlteracao(Versao versao)
    {
        var sb = new StringBuilder();
        sb.AppendLine("--*- encoding: utf-8 -*-");
        sb.AppendLine($"-- Versão: {versao.Numero}");
        sb.AppendLine($"-- Data de geração: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();

        var scriptsOrdenados = versao.VersaoScripts
            .Select(vs => vs.Script)
            .OrderBy(s => s.NumeroOS)
            .ThenBy(s => s.OrdemExecucao)
            .ToList();

        string? osAtual = null;
        foreach (var script in scriptsOrdenados)
        {
            var os = script.NumeroOS ?? "SEM OS";
            if (os != osAtual)
            {
                if (osAtual is not null) sb.AppendLine();
                var header = !string.IsNullOrWhiteSpace(script.NumeroOS)
                    ? $"---- OS {script.NumeroOS}"
                    : "---- SEM OS";
                if (!string.IsNullOrWhiteSpace(script.Branch))
                    header += $" | Branch: {script.Branch}";
                header += " ----";
                sb.AppendLine(header);
                sb.AppendLine();
                osAtual = os;
            }

            if (!string.IsNullOrWhiteSpace(script.Descricao))
                sb.AppendLine($"-- {script.Descricao}");

            sb.AppendLine(script.ScriptSql.TrimEnd());
            sb.AppendLine("GO");
            sb.AppendLine();
        }

        return [.. Utf8Bom, .. Encoding.UTF8.GetBytes(sb.ToString())];
    }

    private static string EscapeSql(string value)
    {
        return value.Replace("'", "''");
    }
}
