namespace PontoScripts.Models;

public class VersaoScript
{
    public int VersaoId { get; set; }
    public int ScriptAlteracaoEntryId { get; set; }

    public Versao Versao { get; set; } = null!;
    public ScriptAlteracaoEntry Script { get; set; } = null!;
}
