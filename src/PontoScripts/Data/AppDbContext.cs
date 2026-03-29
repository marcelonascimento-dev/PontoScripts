using PontoScripts.Models;
using Microsoft.EntityFrameworkCore;

namespace PontoScripts.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GlobalizacaoEntry> GlobalizacaoEntries => Set<GlobalizacaoEntry>();
    public DbSet<ScriptAlteracaoEntry> ScriptAlteracaoEntries => Set<ScriptAlteracaoEntry>();
    public DbSet<Versao> Versoes => Set<Versao>();
    public DbSet<VersaoGlobalizacao> VersaoGlobalizacoes => Set<VersaoGlobalizacao>();
    public DbSet<VersaoScript> VersaoScripts => Set<VersaoScript>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlobalizacaoEntry>(e =>
        {
            e.Property(g => g.Tipo).HasMaxLength(450);
            e.Property(g => g.Mensagem).HasMaxLength(450);
            e.Property(g => g.AtributoAdicional).HasMaxLength(200);
            e.HasIndex(g => new { g.Tipo, g.Mensagem }).IsUnique();
        });

        modelBuilder.Entity<Versao>(e =>
        {
            e.Property(v => v.Status).HasConversion<string>();
        });

        modelBuilder.Entity<VersaoGlobalizacao>(e =>
        {
            e.HasKey(vg => new { vg.VersaoId, vg.GlobalizacaoEntryId });
            e.HasOne(vg => vg.Versao).WithMany(v => v.VersaoGlobalizacoes).HasForeignKey(vg => vg.VersaoId);
            e.HasOne(vg => vg.GlobalizacaoEntry).WithMany(g => g.VersaoGlobalizacoes).HasForeignKey(vg => vg.GlobalizacaoEntryId);
        });

        modelBuilder.Entity<VersaoScript>(e =>
        {
            e.HasKey(vs => new { vs.VersaoId, vs.ScriptAlteracaoEntryId });
            e.HasOne(vs => vs.Versao).WithMany(v => v.VersaoScripts).HasForeignKey(vs => vs.VersaoId);
            e.HasOne(vs => vs.Script).WithMany(s => s.VersaoScripts).HasForeignKey(vs => vs.ScriptAlteracaoEntryId);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(450);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Perfil).HasConversion<string>();
        });
    }
}
