using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bud.Mcp.Auth;
using Bud.Shared.Contracts;

namespace Bud.Mcp.Clients;

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

    public Task<PagedResult<MissionResponse>> ListMissionsAsync(MissionFilter? filter, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<MissionResponse>>(BuildQueryPath(
            "/api/missions",
            ("filter", filter?.ToString()),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<MissionResponse> UpdateMissionAsync(Guid id, PatchMissionRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchMissionRequest, MissionResponse>($"/api/missions/{id}", request, cancellationToken);

    public Task DeleteMissionAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/missions/{id}", cancellationToken);

    public Task<IndicatorResponse> CreateMissionIndicatorAsync(CreateIndicatorRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateIndicatorRequest, IndicatorResponse>("/api/indicators", request, cancellationToken);

    public Task<IndicatorResponse> GetMissionIndicatorAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync<IndicatorResponse>($"/api/indicators/{id}", cancellationToken);

    public Task<PagedResult<IndicatorResponse>> ListMissionIndicatorsAsync(Guid? missionId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<IndicatorResponse>>(BuildQueryPath(
            "/api/indicators",
            ("missionId", missionId?.ToString()),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<IndicatorResponse> UpdateMissionIndicatorAsync(Guid id, PatchIndicatorRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchIndicatorRequest, IndicatorResponse>($"/api/indicators/{id}", request, cancellationToken);

    public Task DeleteMissionIndicatorAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/indicators/{id}", cancellationToken);

    public Task<CheckinResponse> CreateIndicatorCheckinAsync(Guid indicatorId, CreateCheckinRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateCheckinRequest, CheckinResponse>($"/api/indicators/{indicatorId}/checkins", request, cancellationToken);

    public Task<CheckinResponse> GetIndicatorCheckinAsync(Guid indicatorId, Guid id, CancellationToken cancellationToken = default)
        => GetAsync<CheckinResponse>($"/api/indicators/{indicatorId}/checkins/{id}", cancellationToken);

    public Task<PagedResult<CheckinResponse>> ListIndicatorCheckinsAsync(Guid indicatorId, int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAsync<PagedResult<CheckinResponse>>(BuildQueryPath(
            $"/api/indicators/{indicatorId}/checkins",
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), cancellationToken);

    public Task<CheckinResponse> UpdateIndicatorCheckinAsync(Guid indicatorId, Guid id, PatchCheckinRequest request, CancellationToken cancellationToken = default)
        => PatchAsync<PatchCheckinRequest, CheckinResponse>($"/api/indicators/{indicatorId}/checkins/{id}", request, cancellationToken);

    public Task DeleteIndicatorCheckinAsync(Guid indicatorId, Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"/api/indicators/{indicatorId}/checkins/{id}", cancellationToken);

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
