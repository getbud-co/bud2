using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bud.Mcp.Auth;
using Bud.Shared.Contracts;

namespace Bud.Mcp.Http;

public sealed class BudApiClient(HttpClient httpClient, BudApiSession session)
{
    private static readonly JsonSerializerOptions RequestJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly BudApiSession _session = session;

    public Task<MissionResponse> CreateMissionAsync(CreateMissionRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateMissionRequest, MissionResponse>("/api/missions", request, cancellationToken);

    public Task<MissionResponse> GetMissionAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync<MissionResponse>($"/api/missions/{id}", cancellationToken);

    public Task<PagedResult<MissionResponse>> ListMissionsAsync(MissionScopeType? scopeType, Guid? scopeId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<MissionResponse>>(BuildQueryPath(
            "/api/missions",
            ("scopeType", scopeType?.ToString()),
            ("scopeId", scopeId?.ToString()),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<MissionResponse> UpdateMissionAsync(Guid id, PatchMissionRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchMissionRequest, MissionResponse>($"/api/missions/{id}", request, cancellationToken);

    public Task DeleteMissionAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/missions/{id}", cancellationToken);

    public Task<MetricResponse> CreateMissionMetricAsync(CreateMetricRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateMetricRequest, MetricResponse>("/api/metrics", request, cancellationToken);

    public Task<MetricResponse> GetMissionMetricAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync<MetricResponse>($"/api/metrics/{id}", cancellationToken);

    public Task<PagedResult<MetricResponse>> ListMissionMetricsAsync(Guid? missionId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<MetricResponse>>(BuildQueryPath(
            "/api/metrics",
            ("missionId", missionId?.ToString()),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<MetricResponse> UpdateMissionMetricAsync(Guid id, PatchMetricRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchMetricRequest, MetricResponse>($"/api/metrics/{id}", request, cancellationToken);

    public Task DeleteMissionMetricAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/metrics/{id}", cancellationToken);

    public Task<MetricCheckinResponse> CreateMetricCheckinAsync(Guid metricId, CreateCheckinRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateCheckinRequest, MetricCheckinResponse>($"/api/metrics/{metricId}/checkins", request, cancellationToken);

    public Task<MetricCheckinResponse> GetMetricCheckinAsync(Guid metricId, Guid id, CancellationToken cancellationToken = default)
        => GetAsync<MetricCheckinResponse>($"/api/metrics/{metricId}/checkins/{id}", cancellationToken);

    public Task<PagedResult<MetricCheckinResponse>> ListMetricCheckinsAsync(Guid metricId, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<MetricCheckinResponse>>(BuildQueryPath(
            $"/api/metrics/{metricId}/checkins",
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<MetricCheckinResponse> UpdateMetricCheckinAsync(Guid metricId, Guid id, PatchCheckinRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchCheckinRequest, MetricCheckinResponse>($"/api/metrics/{metricId}/checkins/{id}", request, cancellationToken);

    public Task DeleteMetricCheckinAsync(Guid metricId, Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/metrics/{metricId}/checkins/{id}", cancellationToken);

    private async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken)
    {
        using var request = _session.CreateDomainRequest(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadSuccessResponseOrThrowAsync<TResponse>(response, cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest payload, CancellationToken cancellationToken)
    {
        using var request = _session.CreateDomainRequest(HttpMethod.Post, path);
        request.Content = JsonContent.Create(payload, options: RequestJsonOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadSuccessResponseOrThrowAsync<TResponse>(response, cancellationToken);
    }

    private async Task<TResponse> PatchAsync<TRequest, TResponse>(string path, TRequest payload, CancellationToken cancellationToken)
    {
        using var request = _session.CreateDomainRequest(HttpMethod.Patch, path);
        request.Content = JsonContent.Create(payload, options: RequestJsonOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadSuccessResponseOrThrowAsync<TResponse>(response, cancellationToken);
    }

    private async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        using var request = _session.CreateDomainRequest(HttpMethod.Delete, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await BudApiException.FromHttpResponseAsync(response, cancellationToken);
        }
    }

    private static async Task<T> ReadSuccessResponseOrThrowAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await BudApiException.FromHttpResponseAsync(response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<T>(ResponseJsonOptions, cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException("Resposta da API Bud inválida ou vazia.");
        }

        return payload;
    }

    private static string BuildQueryPath(string basePath, params (string Name, string? Value)[] parameters)
    {
        var query = new StringBuilder();
        foreach (var (name, value) in parameters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            query.Append(query.Length == 0 ? '?' : '&');
            query.Append(Uri.EscapeDataString(name));
            query.Append('=');
            query.Append(Uri.EscapeDataString(value));
        }

        return $"{basePath}{query}";
    }
}
