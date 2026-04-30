namespace Bud.Application.Common;

internal static class UserErrorMessages
{
    public const string OrganizationNotFound = "Organização não encontrada.";
    public const string LeaderNotFound = "Líder não encontrado.";
    public const string LeaderMustHaveRole = "O funcionário selecionado deve ter o perfil de Líder.";
    public const string LeaderMustBelongSameOrganization = "O líder deve pertencer à mesma organização.";
    public const string NotificationNotFound = "Notificação não encontrada.";
    public const string AuthenticationFailed = "Falha ao autenticar.";
    public const string ListOrganizationsFailed = "Falha ao carregar organizações.";
    public const string CycleNotFound = "Ciclo não encontrado.";
    public const string CycleCreateForbidden = "Você não tem permissão para criar ciclos nesta organização.";
    public const string CycleUpdateForbidden = "Você não tem permissão para atualizar ciclos nesta organização.";
    public const string CycleDeleteForbidden = "Você não tem permissão para excluir ciclos nesta organização.";
    public const string CycleListForbidden = "Você não tem permissão para listar ciclos nesta organização.";
}
