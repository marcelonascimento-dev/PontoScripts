using PontoScripts.Data;
using PontoScripts.Models;
using Microsoft.EntityFrameworkCore;

namespace PontoScripts.Services;

public class VersaoService(AppDbContext db)
{
    public async Task<List<Versao>> ListarAsync()
    {
        return await db.Versoes
            .Include(v => v.VersaoGlobalizacoes).ThenInclude(vg => vg.GlobalizacaoEntry)
            .Include(v => v.VersaoScripts).ThenInclude(vs => vs.Script)
            .OrderByDescending(v => v.DataCriacao)
            .ToListAsync();
    }

    public async Task<Versao?> ObterAsync(int id)
    {
        return await db.Versoes
            .Include(v => v.VersaoGlobalizacoes).ThenInclude(vg => vg.GlobalizacaoEntry)
            .Include(v => v.VersaoScripts).ThenInclude(vs => vs.Script)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Versao> CriarAsync(Versao versao, List<int> globalizacaoIds, List<int> scriptIds)
    {
        versao.DataCriacao = DateTime.Now;
        versao.Status = StatusVersao.Aberta;
        versao.VersaoGlobalizacoes = globalizacaoIds.Select(id => new VersaoGlobalizacao { GlobalizacaoEntryId = id }).ToList();
        versao.VersaoScripts = scriptIds.Select(id => new VersaoScript { ScriptAlteracaoEntryId = id }).ToList();
        db.Versoes.Add(versao);
        await db.SaveChangesAsync();
        return versao;
    }

    public async Task MarcarComoGeradaAsync(int id)
    {
        var versao = await db.Versoes.FindAsync(id);
        if (versao is not null)
        {
            versao.Status = StatusVersao.Gerada;
            versao.DataGeracao = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    public async Task MarcarComoLiberadaAsync(int id)
    {
        var versao = await db.Versoes.FindAsync(id);
        if (versao is not null)
        {
            versao.Status = StatusVersao.Liberada;
            await db.SaveChangesAsync();
        }
    }

    public async Task ExcluirAsync(int id)
    {
        var versao = await db.Versoes
            .Include(v => v.VersaoGlobalizacoes)
            .Include(v => v.VersaoScripts)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (versao is not null)
        {
            db.Versoes.Remove(versao);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> ContarAbertasAsync()
    {
        return await db.Versoes.CountAsync(v => v.Status == StatusVersao.Aberta);
    }
}
