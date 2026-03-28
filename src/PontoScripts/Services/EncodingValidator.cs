using System.Text;

namespace PontoScripts.Services;

public static class EncodingValidator
{
    private static readonly string[] MojibakePatterns =
    [
        "Ã£", "Ã¡", "Ã©", "Ã­", "Ã³", "Ãº", "Ã§", "Ã±",
        "Ã¢", "Ãª", "Ã®", "Ã´", "Ã»", "Ã€", "Ã‰", "Ã"
    ];

    public static List<string> Validar(string texto, string nomeCampo)
    {
        var erros = new List<string>();

        if (string.IsNullOrEmpty(texto))
            return erros;

        if (texto.Contains('\uFFFD'))
            erros.Add($"{nomeCampo}: contém caractere de substituição Unicode (U+FFFD). Verifique o encoding.");

        foreach (var pattern in MojibakePatterns)
        {
            if (texto.Contains(pattern))
            {
                erros.Add($"{nomeCampo}: possível erro de encoding detectado ('{pattern}'). Verifique se o texto está em UTF-8.");
                break;
            }
        }

        var bytes = Encoding.UTF8.GetBytes(texto);
        var roundTrip = Encoding.UTF8.GetString(bytes);
        if (roundTrip != texto)
            erros.Add($"{nomeCampo}: o texto não sobreviveu ao round-trip UTF-8. Verifique caracteres especiais.");

        return erros;
    }

    public static List<string> ValidarGlobalizacao(string tipo, string mensagem, string ptBr, string enUs, string esEs)
    {
        var erros = new List<string>();
        erros.AddRange(Validar(tipo, "Tipo"));
        erros.AddRange(Validar(mensagem, "Mensagem"));
        erros.AddRange(Validar(ptBr, "Tradução pt-BR"));
        erros.AddRange(Validar(enUs, "Tradução en-US"));
        erros.AddRange(Validar(esEs, "Tradução es-ES"));
        return erros;
    }
}
