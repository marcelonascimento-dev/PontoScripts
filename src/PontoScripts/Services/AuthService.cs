using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PontoScripts.Data;
using PontoScripts.Models;

namespace PontoScripts.Services;

public class AuthService(AppDbContext db)
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public async Task<Usuario?> ValidarLoginAsync(string email, string senha)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

        if (usuario is null) return null;
        if (!VerificarSenha(senha, usuario.SenhaHash)) return null;

        return usuario;
    }

    public async Task<Usuario> CriarUsuarioAsync(string nome, string email, string senha, PerfilUsuario perfil)
    {
        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            SenhaHash = HashSenha(senha),
            Perfil = perfil,
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task<List<Usuario>> ListarAsync()
    {
        return await db.Usuarios.OrderBy(u => u.Nome).ToListAsync();
    }

    public async Task<Usuario?> ObterAsync(int id)
    {
        return await db.Usuarios.FindAsync(id);
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        db.Usuarios.Update(usuario);
        await db.SaveChangesAsync();
    }

    public async Task AlterarSenhaAsync(int id, string novaSenha)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is not null)
        {
            usuario.SenhaHash = HashSenha(novaSenha);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteAlgumUsuarioAsync()
    {
        return await db.Usuarios.AnyAsync();
    }

    private static string HashSenha(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerificarSenha(string senha, string senhaHash)
    {
        var partes = senhaHash.Split('.');
        if (partes.Length != 2) return false;

        var salt = Convert.FromBase64String(partes[0]);
        var hash = Convert.FromBase64String(partes[1]);
        var hashTeste = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, hashTeste);
    }
}
