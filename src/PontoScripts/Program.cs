using PontoScripts.Components;
using PontoScripts.Data;
using PontoScripts.Models;
using PontoScripts.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        var dbPath = builder.Configuration.GetValue<string>("DatabasePath")
            ?? Path.Combine(AppContext.BaseDirectory, "gerenciador.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
});

// Services
builder.Services.AddScoped<GlobalizacaoService>();
builder.Services.AddScoped<ScriptAlteracaoService>();
builder.Services.AddScoped<VersaoService>();
builder.Services.AddScoped<SqlGeneratorService>();
builder.Services.AddScoped<ImportExportService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<ConfiguracaoService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AzureDevOpsService>();

// Authentication — Microsoft Entra ID (Azure AD)
var azureAdSection = builder.Configuration.GetSection("AzureAd");
if (azureAdSection.Exists() && !string.IsNullOrEmpty(azureAdSection["ClientId"]))
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(azureAdSection);

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;
    });
}
else
{
    // Sem Azure AD configurado — acesso livre
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
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

// Auth endpoints
app.MapGet("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// Export endpoints
app.MapGet("/api/export/globalizacao", async (ImportExportService svc) =>
{
    var bytes = await svc.ExportarGlobalizacoesAsync();
    return Results.File(bytes, "application/json", "globalizacoes.json");
});

app.MapGet("/api/export/scripts", async (ImportExportService svc) =>
{
    var bytes = await svc.ExportarScriptsAsync();
    return Results.File(bytes, "application/json", "scripts.json");
});

// Validation endpoint — used by Azure DevOps build validation pipeline
// Query params: verificarScripts=true&verificarGlobalizacao=true
app.MapGet("/api/validacao/branch/{branch}", async (
    string branch,
    bool? verificarScripts,
    bool? verificarGlobalizacao,
    AppDbContext db) =>
{
    var erros = new List<string>();

    var qtdScripts = await db.ScriptAlteracaoEntries
        .CountAsync(s => s.Branch != null && s.Branch == branch);

    var qtdGlobalizacoes = await db.GlobalizacaoEntries
        .CountAsync(g => g.Branch != null && g.Branch == branch);

    if (verificarScripts == true && qtdScripts == 0)
        erros.Add("Alterações em mapeamentos NHibernate detectadas, mas nenhum script de alteração de banco foi cadastrado para a branch.");

    if (verificarGlobalizacao == true && qtdGlobalizacoes == 0)
        erros.Add("Novas globalizações detectadas no código, mas nenhuma entrada de globalização foi cadastrada para a branch.");

    return Results.Ok(new
    {
        branch,
        aprovado = erros.Count == 0,
        scripts = new { cadastrados = qtdScripts },
        globalizacoes = new { cadastradas = qtdGlobalizacoes },
        erros
    });
}).DisableAntiforgery();

// Import endpoints
app.MapPost("/api/import/globalizacao", async (HttpRequest request, ImportExportService svc) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("arquivo");
    if (file is null || file.Length == 0)
        return Results.BadRequest(new { erro = "Nenhum arquivo enviado." });

    using var stream = file.OpenReadStream();
    var result = await svc.ImportarGlobalizacoesAsync(stream);
    return Results.Ok(result);
}).DisableAntiforgery();

app.MapPost("/api/import/scripts", async (HttpRequest request, ImportExportService svc) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("arquivo");
    if (file is null || file.Length == 0)
        return Results.BadRequest(new { erro = "Nenhum arquivo enviado." });

    using var stream = file.OpenReadStream();
    var result = await svc.ImportarScriptsAsync(stream);
    return Results.Ok(result);
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
