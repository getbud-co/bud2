using System.Net.Http.Json;
using System.Text.Json;
using Bud.Shared.Contracts;

namespace Bud.Client.Services;

public sealed class ApiClient
{
    private const int MaxPageSize = 100;
    private readonly HttpClient _http;
    private readonly ToastService _toastService;
    private DateTime _lastLoadWarningUtc = DateTime.MinValue;

    public ApiClient(HttpClient http, ToastService toastService)
    {
        _http = http;
        _toastService = toastService;
    }

    public async Task<List<MyOrganizationResponse>?> GetMyOrganizationsAsync()
    {
        return await GetSafeAsync<List<MyOrganizationResponse>>("api/me/organizations");
    }

    public async Task<PagedResult<OrganizationResponse>?> GetOrganizationsAsync(string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/organizations?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<OrganizationResponse>>(url);
    }

    public async Task<OrganizationResponse?> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/organizations", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<OrganizationResponse>();
    }

    public async Task<OrganizationResponse?> UpdateOrganizationAsync(Guid id, PatchOrganizationRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/organizations/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<OrganizationResponse>();
    }

    public async Task DeleteOrganizationAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/organizations/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<List<CollaboratorLeaderResponse>?> GetLeadersAsync(Guid? organizationId = null)
    {
        var url = "api/collaborators/leaders";
        if (organizationId.HasValue)
        {
            url += $"?organizationId={organizationId.Value}";
        }
        return await GetSafeAsync<List<CollaboratorLeaderResponse>>(url);
    }

    public async Task<PagedResult<WorkspaceResponse>?> GetWorkspacesAsync(Guid? organizationId, string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (organizationId.HasValue)
        {
            queryParams.Add($"organizationId={organizationId.Value}");
        }
        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/workspaces?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<WorkspaceResponse>>(url);
    }

    public async Task<PagedResult<CollaboratorResponse>?> GetOrganizationCollaboratorsAsync(Guid organizationId, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);
        var url = $"api/organizations/{organizationId}/collaborators?page={page}&pageSize={pageSize}";
        return await GetSafeAsync<PagedResult<CollaboratorResponse>>(url);
    }

    public async Task<WorkspaceResponse?> CreateWorkspaceAsync(CreateWorkspaceRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/workspaces", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
    }

    public async Task<WorkspaceResponse?> UpdateWorkspaceAsync(Guid id, PatchWorkspaceRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/workspaces/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
    }

    public async Task DeleteWorkspaceAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/workspaces/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<PagedResult<TeamResponse>?> GetTeamsAsync(Guid? workspaceId, Guid? parentTeamId, string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (workspaceId.HasValue)
        {
            queryParams.Add($"workspaceId={workspaceId.Value}");
        }
        if (parentTeamId.HasValue)
        {
            queryParams.Add($"parentTeamId={parentTeamId.Value}");
        }
        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/teams?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<TeamResponse>>(url);
    }

    public async Task<TeamResponse?> CreateTeamAsync(CreateTeamRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/teams", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<TeamResponse>();
    }

    public async Task<TeamResponse?> UpdateTeamAsync(Guid id, PatchTeamRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/teams/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<TeamResponse>();
    }

    public async Task DeleteTeamAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/teams/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<PagedResult<CollaboratorResponse>?> GetCollaboratorsAsync(Guid? teamId, string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (teamId.HasValue)
        {
            queryParams.Add($"teamId={teamId.Value}");
        }
        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/collaborators?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<CollaboratorResponse>>(url);
    }

    public async Task<CollaboratorResponse?> CreateCollaboratorAsync(CreateCollaboratorRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/collaborators", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<CollaboratorResponse>();
    }

    public async Task<CollaboratorResponse?> UpdateCollaboratorAsync(Guid id, PatchCollaboratorRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/collaborators/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<CollaboratorResponse>();
    }

    public async Task DeleteCollaboratorAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/collaborators/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<PagedResult<MissionResponse>?> GetMissionsAsync(
        MissionScopeType? scopeType,
        Guid? scopeId,
        string? search,
        int page = 1,
        int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (scopeType.HasValue)
        {
            queryParams.Add($"scopeType={scopeType.Value}");
        }
        if (scopeId.HasValue)
        {
            queryParams.Add($"scopeId={scopeId.Value}");
        }
        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/missions?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<MissionResponse>>(url);
    }

    public async Task<PagedResult<MissionResponse>?> GetMyMissionsAsync(
        string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/me/missions?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<MissionResponse>>(url);
    }

    public async Task<MissionResponse?> CreateMissionAsync(CreateMissionRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/missions", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MissionResponse>();
    }

    public async Task<MyDashboardResponse?> GetMyDashboardAsync(Guid? teamId = null)
    {
        var url = "api/me/dashboard";
        if (teamId.HasValue)
        {
            url += $"?teamId={teamId.Value}";
        }
        return await GetSafeAsync<MyDashboardResponse>(url);
    }

    public async Task<PagedResult<MetricResponse>?> GetMissionMetricsAsync(Guid? missionId, string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (missionId.HasValue)
        {
            queryParams.Add($"missionId={missionId.Value}");
        }
        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/metrics?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<MetricResponse>>(url);
    }

    public async Task<MetricResponse?> CreateMissionMetricAsync(CreateMetricRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/metrics", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MetricResponse>();
    }

    public async Task<MissionResponse?> UpdateMissionAsync(Guid id, PatchMissionRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/missions/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MissionResponse>();
    }

    public async Task DeleteMissionAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/missions/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<MetricResponse?> UpdateMissionMetricAsync(Guid id, PatchMetricRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/metrics/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MetricResponse>();
    }

    public async Task DeleteMissionMetricAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/metrics/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<PagedResult<MetricResponse>?> GetMissionMetricsByMissionIdAsync(Guid missionId, int page = 1, int pageSize = 100)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);
        var url = $"api/missions/{missionId}/metrics?page={page}&pageSize={pageSize}";
        return await GetSafeAsync<PagedResult<MetricResponse>>(url);
    }

    // Objective methods
    public async Task<PagedResult<ObjectiveResponse>?> GetMissionObjectivesAsync(Guid missionId, int page = 1, int pageSize = 100)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>
        {
            $"missionId={missionId}",
            $"page={page}",
            $"pageSize={pageSize}"
        };

        var url = $"api/objectives?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<ObjectiveResponse>>(url);
    }

    public async Task<ObjectiveResponse?> CreateMissionObjectiveAsync(CreateObjectiveRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/objectives", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<ObjectiveResponse>();
    }

    public async Task<ObjectiveResponse?> UpdateMissionObjectiveAsync(Guid id, PatchObjectiveRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/objectives/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<ObjectiveResponse>();
    }

    public async Task DeleteMissionObjectiveAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/objectives/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<List<ObjectiveProgressResponse>?> GetObjectiveProgressAsync(List<Guid> objectiveIds)
    {
        if (objectiveIds.Count == 0)
        {
            return [];
        }

        var ids = string.Join(",", objectiveIds);
        return await GetSafeAsync<List<ObjectiveProgressResponse>>($"api/objectives/progress?ids={ids}");
    }

    // Metric Progress
    public async Task<List<MetricProgressResponse>?> GetMetricProgressAsync(List<Guid> metricIds)
    {
        if (metricIds.Count == 0)
        {
            return [];
        }

        var ids = string.Join(",", metricIds);
        return await GetSafeAsync<List<MetricProgressResponse>>($"api/metrics/progress?ids={ids}");
    }

    // Mission Progress
    public async Task<List<MissionProgressResponse>?> GetMissionProgressAsync(List<Guid> missionIds)
    {
        if (missionIds.Count == 0)
        {
            return [];
        }

        var ids = string.Join(",", missionIds);
        return await GetSafeAsync<List<MissionProgressResponse>>($"api/missions/progress?ids={ids}");
    }

    // MetricCheckin methods
    public async Task<PagedResult<MetricCheckinResponse>?> GetMetricCheckinsAsync(Guid metricId, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/metrics/{metricId}/checkins?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<MetricCheckinResponse>>(url);
    }

    public async Task<MetricCheckinResponse?> CreateMetricCheckinAsync(Guid metricId, CreateCheckinRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/metrics/{metricId}/checkins", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MetricCheckinResponse>();
    }

    public async Task<MetricCheckinResponse?> UpdateMetricCheckinAsync(Guid metricId, Guid id, PatchCheckinRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/metrics/{metricId}/checkins/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<MetricCheckinResponse>();
    }

    public async Task DeleteMetricCheckinAsync(Guid metricId, Guid id)
    {
        var response = await _http.DeleteAsync($"api/metrics/{metricId}/checkins/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    // Template methods
    public async Task<PagedResult<TemplateResponse>?> GetTemplatesAsync(string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/templates?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<TemplateResponse>>(url);
    }

    public async Task<TemplateResponse?> GetTemplateByIdAsync(Guid id)
    {
        return await GetSafeAsync<TemplateResponse>($"api/templates/{id}");
    }

    public async Task<TemplateResponse?> CreateTemplateAsync(CreateTemplateRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/templates", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<TemplateResponse>();
    }

    public async Task<TemplateResponse?> UpdateTemplateAsync(Guid id, PatchTemplateRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/templates/{id}", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<TemplateResponse>();
    }

    public async Task DeleteTemplateAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/templates/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    private async Task<T?> GetSafeAsync<T>(string url)
    {
        try
        {
            return await _http.GetFromJsonAsync<T>(url);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro ao carregar dados ({url}): {ex.Message}");
            ShowLoadWarningThrottled();
            return default;
        }
    }

    private void ShowLoadWarningThrottled()
    {
        if ((DateTime.UtcNow - _lastLoadWarningUtc).TotalSeconds < 3)
        {
            return;
        }

        _lastLoadWarningUtc = DateTime.UtcNow;
        _toastService.ShowWarning("Falha ao carregar dados",
            "Não foi possível carregar os dados. Verifique sua conexão e tente novamente.");
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            string body = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            // ValidationProblemDetails — extract messages from "errors" dict
            if (root.TryGetProperty("errors", out JsonElement errors))
            {
                List<string> messages = new();
                foreach (JsonProperty field in errors.EnumerateObject())
                {
                    foreach (JsonElement msg in field.Value.EnumerateArray())
                    {
                        messages.Add(msg.GetString() ?? string.Empty);
                    }
                }

                if (messages.Count > 0)
                {
                    return string.Join(" ", messages);
                }
            }

            // ProblemDetails — extract "detail"
            if (root.TryGetProperty("detail", out JsonElement detail))
            {
                string? detailText = detail.GetString();
                if (!string.IsNullOrWhiteSpace(detailText))
                {
                    return detailText;
                }
            }
        }
        catch
        {
            // Fallback if response body can't be parsed
        }

        return $"Erro do servidor ({(int)response.StatusCode}).";
    }

    public async Task<SessionResponse?> LoginAsync(CreateSessionRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/sessions", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SessionResponse>();
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
            response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return null;
        }

        var errorMessage = await ExtractErrorMessageAsync(response);
        throw new HttpRequestException(errorMessage);
    }

    public async Task LogoutAsync()
    {
        var response = await _http.DeleteAsync("api/sessions/current");
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    // Collaborator Subordinates (Hierarchy)
    public async Task<List<CollaboratorSubordinateResponse>?> GetCollaboratorSubordinatesAsync(Guid collaboratorId)
    {
        return await GetSafeAsync<List<CollaboratorSubordinateResponse>>($"api/collaborators/{collaboratorId}/subordinates");
    }

    // Collaborator Teams (Many-to-Many)
    public async Task<List<CollaboratorTeamResponse>?> GetCollaboratorTeamsAsync(Guid collaboratorId)
    {
        return await GetSafeAsync<List<CollaboratorTeamResponse>>($"api/collaborators/{collaboratorId}/teams");
    }

    public async Task UpdateCollaboratorTeamsAsync(Guid collaboratorId, PatchCollaboratorTeamsRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/collaborators/{collaboratorId}/teams", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<List<CollaboratorTeamResponse>?> GetAvailableTeamsForCollaboratorAsync(Guid collaboratorId, string? search = null)
    {
        var url = $"api/collaborators/{collaboratorId}/teams/eligible-for-assignment";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }
        return await GetSafeAsync<List<CollaboratorTeamResponse>>(url);
    }

    // Collaborator Summaries
    public async Task<List<CollaboratorLookupResponse>?> GetCollaboratorLookupAsync(string? search = null)
    {
        var url = "api/collaborators/lookup";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }
        return await GetSafeAsync<List<CollaboratorLookupResponse>>(url);
    }

    // Team Collaborators (Many-to-Many)
    public async Task<List<CollaboratorLookupResponse>?> GetTeamCollaboratorSummariesAsync(Guid teamId)
    {
        return await GetSafeAsync<List<CollaboratorLookupResponse>>($"api/teams/{teamId}/collaborators/lookup");
    }

    public async Task UpdateTeamCollaboratorsAsync(Guid teamId, PatchTeamCollaboratorsRequest request)
    {
        var response = await _http.PatchAsJsonAsync($"api/teams/{teamId}/collaborators", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task<List<CollaboratorLookupResponse>?> GetAvailableCollaboratorsForTeamAsync(Guid teamId, string? search = null)
    {
        var url = $"api/teams/{teamId}/collaborators/eligible-for-assignment";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }
        return await GetSafeAsync<List<CollaboratorLookupResponse>>(url);
    }

    // NotificationResponse methods
    public async Task<PagedResult<NotificationResponse>?> GetNotificationsAsync(bool? isRead = null, int page = 1, int pageSize = 20)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);
        var queryParams = new List<string>();
        if (isRead.HasValue)
        {
            queryParams.Add($"isRead={isRead.Value.ToString().ToLowerInvariant()}");
        }

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");
        var url = $"api/notifications?{string.Join("&", queryParams)}";
        return await GetSafeAsync<PagedResult<NotificationResponse>>(url);
    }

    public async Task MarkNotificationAsReadAsync(Guid id)
    {
        var response = await _http.PatchAsync($"api/notifications/{id}", null);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    public async Task MarkAllNotificationsAsReadAsync()
    {
        var response = await _http.PatchAsync("api/notifications", null);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response);
            throw new HttpRequestException(errorMessage);
        }
    }

    private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return (normalizedPage, normalizedPageSize);
    }
}
