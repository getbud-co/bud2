# Value Object Mapping Guardrails

Matriz declarativa para garantir que serviços façam mapeamento explícito de campos críticos de request para Value Objects.

Formato por linha:
`<arquivo>|<required-1>||<required-2>|<forbidden-1>||<forbidden-2>`

Linhas iniciadas com `#` são comentários.

src/Api/Bud.Application/Features/Employees/UseCases/CreateEmployee.cs|EmailAddress.TryCreate(command.Email)||EmployeeName.TryCreate(command.FullName)||Employee.Create(|
src/Api/Bud.Application/Features/Employees/UseCases/UpdateEmployee.cs|EmployeeName.TryCreate(command.FullName.Value)||EmailAddress.TryCreate(command.Email.Value)||UpdateProfile(|
src/Api/Bud.Application/Features/Organizations/UseCases/CreateOrganization.cs|Organization.Create(|
src/Api/Bud.Application/Features/Organizations/UseCases/UpdateOrganization.cs|organization.Rename(|
src/Api/Bud.Application/Features/Indicators/UseCases/CreateCheckin.cs|indicator.CreateCheckin(|
src/Api/Bud.Application/Features/Indicators/UseCases/PatchCheckin.cs|indicator.UpdateCheckin(|
src/Api/Bud.Application/Features/Missions/UseCases/CreateMission.cs|mission.EmployeeId|
src/Api/Bud.Application/Features/Missions/UseCases/PatchMission.cs|mission.EmployeeId|
