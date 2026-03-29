namespace PontoScripts.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Dev;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

public enum PerfilUsuario
{
    Admin,
    Dev,
    Operacao
}
