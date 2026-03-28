namespace PontoScripts.Models;

public class VersaoGlobalizacao
{
    public int VersaoId { get; set; }
    public int GlobalizacaoEntryId { get; set; }

    public Versao Versao { get; set; } = null!;
    public GlobalizacaoEntry GlobalizacaoEntry { get; set; } = null!;
}
