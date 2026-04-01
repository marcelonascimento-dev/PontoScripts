using PontoScripts.Data;
using PontoScripts.Models;
using Microsoft.EntityFrameworkCore;

namespace PontoScripts.Services;

public class ScriptAlteracaoService(AppDbContext db)
{
    public async Task<List<ScriptAlteracaoEntry>> ListarAsync(string? busca = null)
    {
        var query = db.ScriptAlteracaoEntries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.ToLower();
            query = query.Where(s => (s.NumeroOS != null && s.NumeroOS.ToLower().Contains(termo))
                || s.Descricao.ToLower().Contains(termo)
                || (s.Branch != null && s.Branch.ToLower().Contains(termo)));
        }
        return await query
            .Include(s => s.VersaoScripts)
            .OrderBy(s => s.OrdemExecucao)
            .ToListAsync();
    }

    public async Task<ScriptAlteracaoEntry?> ObterAsync(int id)
    {
        return await db.ScriptAlteracaoEntries.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ScriptAlteracaoEntry> CriarAsync(ScriptAlteracaoEntry entry)
    {
        entry.DataCriacao = DateTime.Now;
        if (entry.OrdemExecucao == 0)
        {
            var maxOrdem = await db.ScriptAlteracaoEntries
                .MaxAsync(s => (int?)s.OrdemExecucao) ?? 0;
            entry.OrdemExecucao = maxOrdem + 1;
        }
        db.ScriptAlteracaoEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task AtualizarAsync(ScriptAlteracaoEntry entry)
    {
        db.ScriptAlteracaoEntries.Update(entry);
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var entry = await db.ScriptAlteracaoEntries.FindAsync(id);
        if (entry is not null)
        {
            db.ScriptAlteracaoEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<ScriptAlteracaoEntry>> ListarPendentesAsync()
    {
        return await db.ScriptAlteracaoEntries
            .Where(s => !s.VersaoScripts.Any())
            .OrderBy(s => s.NumeroOS).ThenBy(s => s.OrdemExecucao)
            .ToListAsync();
    }

    public async Task<int> ContarPendentesAsync()
    {
        return await db.ScriptAlteracaoEntries.CountAsync(s => !s.VersaoScripts.Any());
    }
}
