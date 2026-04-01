using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PontoScripts.Services;

public class AzureDevOpsService
{
    private readonly ConfiguracaoService _configService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AzureDevOpsService> _logger;

    private List<BranchInfo>? _cachedBranches;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public AzureDevOpsService(
        ConfiguracaoService configService,
        IHttpClientFactory httpClientFactory,
        ILogger<AzureDevOpsService> logger)
    {
        _configService = configService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool PatConfigurado => !string.IsNullOrWhiteSpace(_configService.ObterAzureDevOpsPat());

    public string? UltimoErro { get; private set; }

    public async Task<List<BranchInfo>> ListarBranchesAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedBranches is not null && DateTime.UtcNow < _cacheExpiry)
            return _cachedBranches;

        UltimoErro = null;
        var pat = _configService.ObterAzureDevOpsPat();
        var repos = _configService.ObterAzureDevOpsRepos();

        if (string.IsNullOrWhiteSpace(pat) || repos.Count == 0)
            return [];

        var allBranches = new List<BranchInfo>();

        foreach (var repo in repos)
        {
            try
            {
                var branches = await FetchBranchesAsync(repo, pat);
                allBranches.AddRange(branches);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar branches do repositório {Repo}", repo.Nome);
                UltimoErro = $"Erro ao buscar branches de {repo.Nome}: {ex.Message}";
            }
        }

        allBranches = allBranches
            .OrderBy(b => b.Repositorio)
            .ThenBy(b => b.Nome)
            .ToList();

        _cachedBranches = allBranches;
        _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

        return allBranches;
    }

    private async Task<List<BranchInfo>> FetchBranchesAsync(RepoConfig repo, string pat)
    {
        var client = _httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        // Azure DevOps REST API: list refs (branches)
        var url = $"https://dev.azure.com/{repo.Organizacao}/{repo.Projeto}/_apis/git/repositories/{repo.Nome}/refs?filter=heads/&api-version=7.0";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AzureDevOpsRefsResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Value is null) return [];

        return result.Value
            .Select(r => new BranchInfo
            {
                Nome = r.Name.Replace("refs/heads/", ""),
                Repositorio = repo.Nome
            })
            .ToList();
    }

    public void InvalidarCache()
    {
        _cachedBranches = null;
        _cacheExpiry = DateTime.MinValue;
    }

    private class AzureDevOpsRefsResponse
    {
        public List<AzureDevOpsRef>? Value { get; set; }
    }

    private class AzureDevOpsRef
    {
        public string Name { get; set; } = "";
    }
}

public class BranchInfo
{
    public string Nome { get; set; } = "";
    public string Repositorio { get; set; } = "";
}

public class RepoConfig
{
    public string Organizacao { get; set; } = "";
    public string Projeto { get; set; } = "";
    public string Nome { get; set; } = "";
}
