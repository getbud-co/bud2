# Value Object Mapping Guardrails

Matriz declarativa para garantir que serviços façam mapeamento explícito de campos críticos de request para Value Objects.

Formato por linha:
`<arquivo>|<required-1>||<required-2>|<forbidden-1>||<forbidden-2>`

Linhas iniciadas com `#` são comentários.

src/Api/Bud.Application/Features/Employees/UseCases/PatchEmployee.cs|PersonName.TryCreate(requestedFullName||UpdateProfile(|
src/Api/Bud.Application/Features/Indicators/UseCases/CreateCheckin.cs|indicator.CreateCheckin(|
src/Api/Bud.Application/Features/Indicators/UseCases/PatchCheckin.cs|indicator.UpdateCheckin(|
src/Api/Bud.Application/Features/Missions/UseCases/CreateMission.cs|mission.EmployeeId|
src/Api/Bud.Application/Features/Missions/UseCases/PatchMission.cs|mission.EmployeeId|
