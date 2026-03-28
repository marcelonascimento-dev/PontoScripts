using System.Text.RegularExpressions;

namespace PontoScripts.Services;

public static partial class SqlSyntaxValidator
{
    public static List<string> Validar(string sql)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(sql))
        {
            erros.Add("O script SQL não pode estar vazio.");
            return erros;
        }

        var beginCount = CountKeyword(sql, "BEGIN");
        var endCount = CountKeyword(sql, "END");
        if (beginCount != endCount)
            erros.Add($"Blocos BEGIN/END desbalanceados: {beginCount} BEGIN(s) e {endCount} END(s).");

        var openParens = sql.Count(c => c == '(');
        var closeParens = sql.Count(c => c == ')');
        if (openParens != closeParens)
            erros.Add($"Parênteses desbalanceados: {openParens} '(' e {closeParens} ')'.");

        var singleQuotes = sql.Count(c => c == '\'');
        if (singleQuotes % 2 != 0)
            erros.Add("Aspas simples (') desbalanceadas. Verifique se todas as strings estão fechadas.");

        if (HasUnclosedBlockComment(sql))
            erros.Add("Comentário de bloco /* */ não fechado.");

        return erros;
    }

    private static int CountKeyword(string sql, string keyword)
    {
        var pattern = $@"\b{keyword}\b";
        return Regex.Matches(sql, pattern, RegexOptions.IgnoreCase).Count;
    }

    private static bool HasUnclosedBlockComment(string sql)
    {
        int depth = 0;
        for (int i = 0; i < sql.Length - 1; i++)
        {
            if (sql[i] == '/' && sql[i + 1] == '*') { depth++; i++; }
            else if (sql[i] == '*' && sql[i + 1] == '/') { depth--; i++; }
        }
        return depth != 0;
    }
}
