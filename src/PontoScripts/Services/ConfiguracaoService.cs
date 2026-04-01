using System.Text.Json;
using System.Text.Json.Nodes;

namespace PontoScripts.Services;

public class ConfiguracaoService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public ConfiguracaoService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public string ObterProvider()
    {
        return _configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
    }

    public string ObterConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public string ObterCaminhoBancoSqlite()
    {
        return _configuration.GetValue<string>("DatabasePath")
            ?? Path.Combine(AppContext.BaseDirectory, "gerenciador.db");
    }

    public long ObterTamanhoBancoSqlite()
    {
        var path = ObterCaminhoBancoSqlite();
        if (File.Exists(path))
            return new FileInfo(path).Length;
        return 0;
    }

    public string ObterAmbiente() => _env.EnvironmentName;

    public string ObterAppSettingsPath()
    {
        return Path.Combine(_env.ContentRootPath, "appsettings.json");
    }

    // Azure DevOps
    public string ObterAzureDevOpsPat() => _configuration["AzureDevOps:Pat"] ?? "";

    public List<RepoConfig> ObterAzureDevOpsRepos()
    {
        var repos = new List<RepoConfig>();
        var section = _configuration.GetSection("AzureDevOps:Repositorios");
        if (!section.Exists()) return repos;

        foreach (var child in section.GetChildren())
        {
            var org = child["Organizacao"] ?? "";
            var projeto = child["Projeto"] ?? "";
            var nome = child["Nome"] ?? "";
            if (!string.IsNullOrEmpty(org) && !string.IsNullOrEmpty(nome))
                repos.Add(new RepoConfig { Organizacao = org, Projeto = projeto, Nome = nome });
        }
        return repos;
    }

    public async Task SalvarAzureDevOpsAsync(string pat, List<RepoConfig> repos)
    {
        var appSettingsPath = ObterAppSettingsPath();
        JsonNode? root;
        if (File.Exists(appSettingsPath))
        {
            var json = await File.ReadAllTextAsync(appSettingsPath);
            root = JsonNode.Parse(json) ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        var devops = new JsonObject();
        devops["Pat"] = pat;

        var reposArray = new JsonArray();
        foreach (var repo in repos)
        {
            reposArray.Add(new JsonObject
            {
                ["Organizacao"] = repo.Organizacao,
                ["Projeto"] = repo.Projeto,
                ["Nome"] = repo.Nome
            });
        }
        devops["Repositorios"] = reposArray;
        root["AzureDevOps"] = devops;

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(appSettingsPath, root.ToJsonString(options));
    }

    // Azure AD
    public string ObterAzureAdTenantId() => _configuration["AzureAd:TenantId"] ?? "";
    public string ObterAzureAdClientId() => _configuration["AzureAd:ClientId"] ?? "";
    public string ObterAzureAdClientSecret() => _configuration["AzureAd:ClientSecret"] ?? "";
    public bool AzureAdConfigurado() => !string.IsNullOrEmpty(_configuration["AzureAd:ClientId"]);

    public async Task SalvarAzureAdAsync(string tenantId, string clientId, string clientSecret)
    {
        var appSettingsPath = ObterAppSettingsPath();

        JsonNode? root;
        if (File.Exists(appSettingsPath))
        {
            var json = await File.ReadAllTextAsync(appSettingsPath);
            root = JsonNode.Parse(json) ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        var azureAd = root["AzureAd"]?.AsObject() ?? new JsonObject();
        azureAd["Instance"] = "https://login.microsoftonline.com/";
        azureAd["TenantId"] = tenantId;
        azureAd["ClientId"] = clientId;
        azureAd["ClientSecret"] = clientSecret;
        azureAd["CallbackPath"] = "/signin-oidc";
        root["AzureAd"] = azureAd;

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(appSettingsPath, root.ToJsonString(options));
    }

    public async Task SalvarConfiguracaoAsync(string provider, string? connectionString, string? caminhoSqlite)
    {
        var appSettingsPath = ObterAppSettingsPath();

        JsonNode? root;
        if (File.Exists(appSettingsPath))
        {
            var json = await File.ReadAllTextAsync(appSettingsPath);
            root = JsonNode.Parse(json) ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        root["DatabaseProvider"] = provider;

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connStrings = root["ConnectionStrings"]?.AsObject() ?? new JsonObject();
            connStrings["DefaultConnection"] = connectionString;
            root["ConnectionStrings"] = connStrings;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(caminhoSqlite))
                root["DatabasePath"] = caminhoSqlite;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(appSettingsPath, root.ToJsonString(options));
    }
}
