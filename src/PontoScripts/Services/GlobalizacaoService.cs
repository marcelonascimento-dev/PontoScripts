using PontoScripts.Data;
using PontoScripts.Models;
using Microsoft.EntityFrameworkCore;

namespace PontoScripts.Services;

public class GlobalizacaoService(AppDbContext db)
{
    public async Task<List<GlobalizacaoEntry>> ListarAsync(string? busca = null)
    {
        var query = db.GlobalizacaoEntries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.ToLower();
            query = query.Where(g => g.Tipo.ToLower().Contains(termo)
                || g.Mensagem.ToLower().Contains(termo)
                || g.TraducaoPtBR.ToLower().Contains(termo)
                || g.TraducaoEnUS.ToLower().Contains(termo)
                || g.TraducaoEsES.ToLower().Contains(termo)
                || (g.Branch != null && g.Branch.ToLower().Contains(termo)));
        }
        return await query.OrderByDescending(g => g.DataCriacao).ToListAsync();
    }

    public async Task<GlobalizacaoEntry?> ObterAsync(int id)
    {
        return await db.GlobalizacaoEntries.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GlobalizacaoEntry> CriarAsync(GlobalizacaoEntry entry)
    {
        entry.DataCriacao = DateTime.Now;
        db.GlobalizacaoEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task AtualizarAsync(GlobalizacaoEntry entry)
    {
        db.GlobalizacaoEntries.Update(entry);
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var entry = await db.GlobalizacaoEntries.FindAsync(id);
        if (entry is not null)
        {
            db.GlobalizacaoEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteDuplicadaAsync(string tipo, string mensagem, int? excludeId = null)
    {
        var query = db.GlobalizacaoEntries.Where(g => g.Tipo == tipo && g.Mensagem == mensagem);
        if (excludeId.HasValue)
            query = query.Where(g => g.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<List<string>> ListarTiposDistintosAsync()
    {
        return await db.GlobalizacaoEntries.Select(g => g.Tipo).Distinct().OrderBy(t => t).ToListAsync();
    }

    public async Task<List<GlobalizacaoEntry>> ListarPendentesAsync()
    {
        return await db.GlobalizacaoEntries
            .Where(g => !g.VersaoGlobalizacoes.Any())
            .OrderByDescending(g => g.DataCriacao)
            .ToListAsync();
    }

    public async Task<int> ContarPendentesAsync()
    {
        return await db.GlobalizacaoEntries.CountAsync(g => !g.VersaoGlobalizacoes.Any());
    }
}
