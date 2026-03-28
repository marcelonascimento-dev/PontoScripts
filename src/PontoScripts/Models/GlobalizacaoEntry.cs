namespace PontoScripts.Models;

public class GlobalizacaoEntry
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string TraducaoPtBR { get; set; } = string.Empty;
    public string TraducaoEnUS { get; set; } = string.Empty;
    public string TraducaoEsES { get; set; } = string.Empty;
    public string AtributoAdicional { get; set; } = "N";
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public List<VersaoGlobalizacao> VersaoGlobalizacoes { get; set; } = [];
}
