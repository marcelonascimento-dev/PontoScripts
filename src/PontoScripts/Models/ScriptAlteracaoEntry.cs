namespace PontoScripts.Models;

public class ScriptAlteracaoEntry
{
    public int Id { get; set; }
    public string? NumeroOS { get; set; }
    public string? Branch { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string ScriptSql { get; set; } = string.Empty;
    public string CriadoPor { get; set; } = string.Empty;
    public int OrdemExecucao { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public List<VersaoScript> VersaoScripts { get; set; } = [];
}
