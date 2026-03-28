using PontoScripts.Components;
using PontoScripts.Data;
using PontoScripts.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
var dbPath = builder.Configuration.GetValue<string>("DatabasePath")
    ?? Path.Combine(AppContext.BaseDirectory, "gerenciador.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Services
builder.Services.AddScoped<GlobalizacaoService>();
builder.Services.AddScoped<ScriptAlteracaoService>();
builder.Services.AddScoped<VersaoService>();
builder.Services.AddScoped<SqlGeneratorService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

// Download endpoints
app.MapGet("/api/versao/{id:int}/download/globalizacao", async (int id, VersaoService versaoService, SqlGeneratorService sqlGenerator) =>
{
    var versao = await versaoService.ObterAsync(id);
    if (versao is null) return Results.NotFound();

    var bytes = sqlGenerator.GerarGlobalizacao(versao);
    return Results.File(bytes, "application/sql", $"Globalizacao_v{versao.Numero}.sql");
});

app.MapGet("/api/versao/{id:int}/download/scriptalteracao", async (int id, VersaoService versaoService, SqlGeneratorService sqlGenerator) =>
{
    var versao = await versaoService.ObterAsync(id);
    if (versao is null) return Results.NotFound();

    var bytes = sqlGenerator.GerarScriptAlteracao(versao);
    return Results.File(bytes, "application/sql", $"ScriptAlteracaoBanco_v{versao.Numero}.sql");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
