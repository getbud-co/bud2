namespace Bud.Application.Common;

internal static class UserErrorMessages
{
    public const string ScopeNotFound = "Escopo não encontrado.";
    public const string MissionNotFound = "Meta não encontrada.";
    public const string ParentMissionNotFound = "Meta pai não encontrada.";
    public const string MissionCreateForbidden = "Você não tem permissão para criar metas nesta organização.";
    public const string MissionUpdateForbidden = "Você não tem permissão para atualizar metas nesta organização.";
    public const string IndicatorNotFound = "Indicador não encontrado.";
    public const string CheckinNotFound = "Check-in não encontrado.";
    public const string CheckinEditAuthorOnly = "Apenas o autor pode editar este check-in.";
    public const string CheckinDeleteAuthorOnly = "Apenas o autor pode excluir este check-in.";
    public const string TeamNotFound = "Time não encontrado.";
    public const string ParentTeamNotFound = "Time pai não encontrado.";
    public const string TeamCreateForbidden = "Apenas um líder da organização pode criar times.";
    public const string TeamParentMustBeSameOrganization = "O time pai deve pertencer à mesma organização.";
    public const string TeamSelfParentForbidden = "Um time não pode ser seu próprio pai.";
    public const string TeamDeleteWithSubTeamsConflict = "Não é possível excluir um time com sub-times. Exclua os sub-times primeiro.";
    public const string TeamLeaderMustBeMember = "O líder da equipe deve estar incluído na lista de membros.";
    public const string TeamMembersInvalid = "Um ou mais funcionários são inválidos ou pertencem a outra organização.";
    public const string EmployeeNotFound = "Funcionário não encontrado.";
    public const string EmployeeNotIdentified = "Funcionário não identificado.";
    public const string EmployeeContextNotFound = "Contexto de organização não encontrado.";
    public const string EmployeeInvalidEmail = "E-mail inválido.";
    public const string EmployeeNameRequired = "O nome do funcionário é obrigatório.";
    public const string EmployeeEmailAlreadyInUse = "O email já está em uso.";
    public const string EmployeeTeamsInvalid = "Uma ou mais equipes são inválidas ou pertencem a outra organização.";
    public const string OrganizationNotFound = "Organização não encontrada.";
    public const string OrganizationNameConflict = "Já existe uma organização cadastrada com este domínio.";
    public const string LeaderNotFound = "Líder não encontrado.";
    public const string LeaderMustHaveRole = "O funcionário selecionado deve ter o perfil de Líder.";
    public const string LeaderMustBelongSameOrganization = "O líder deve pertencer à mesma organização.";
    public const string TemplateNotFound = "Template não encontrado.";
    public const string TemplateCreateForbidden = "Você não tem permissão para criar templates nesta organização.";
    public const string NotificationNotFound = "Notificação não encontrada.";
    public const string AuthenticationFailed = "Falha ao autenticar.";
    public const string IndicatorProgressCalculationFailed = "Falha ao calcular progresso do indicador.";
    public const string MissionProgressCalculationFailed = "Falha ao calcular progresso das metas.";
    public const string ListOrganizationsFailed = "Falha ao carregar organizações.";
    public const string TaskNotFound = "Tarefa não encontrada.";
}
