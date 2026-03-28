namespace PontoScripts.Models;

public class Versao
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataGeracao { get; set; }
    public StatusVersao Status { get; set; } = StatusVersao.Aberta;

    public List<VersaoGlobalizacao> VersaoGlobalizacoes { get; set; } = [];
    public List<VersaoScript> VersaoScripts { get; set; } = [];
}
