using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Persistence;

public static class DbSeeder
{
    private const string DefaultOrganizationName = "getbud.co";
    private const string DefaultAdminEmail = "admin@getbud.co";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var budOrg = await context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Name == DefaultOrganizationName);

        if (budOrg is null)
        {
            budOrg = Organization.Create(
                Guid.NewGuid(),
                DefaultOrganizationName,
                cnpj: "00.000.000/0001-00",
                OrganizationPlan.Free,
                OrganizationContractStatus.ToApproval);
            context.Organizations.Add(budOrg);
            await context.SaveChangesAsync();
        }

        var adminMember = await context.OrganizationEmployeeMembers
            .IgnoreQueryFilters()
            .Include(m => m.Employee)
            .FirstOrDefaultAsync(m =>
                m.OrganizationId == budOrg.Id &&
                m.Employee.Email == DefaultAdminEmail);

        if (adminMember is null)
        {
            var adminId = Guid.NewGuid();
            var adminEmployee = new Employee
            {
                Id = adminId,
                FullName = "Administrador Global",
                Email = DefaultAdminEmail,
            };
            adminMember = new OrganizationEmployeeMember
            {
                EmployeeId = adminId,
                OrganizationId = budOrg.Id,
                Role = EmployeeRole.TeamLeader,
                IsGlobalAdmin = true,
                Employee = adminEmployee,
            };
            context.Employees.Add(adminEmployee);
            context.OrganizationEmployeeMembers.Add(adminMember);
            await context.SaveChangesAsync();
        }

        if (!adminMember.IsGlobalAdmin)
        {
            adminMember.IsGlobalAdmin = true;
            await context.SaveChangesAsync();
        }

        await SeedTemplatesAsync(context, budOrg.Id);
    }

    private static async Task SeedTemplatesAsync(
        ApplicationDbContext context,
        Guid organizationId)
    {
        var templates = new List<Template>
        {
            BuildBscTemplate(organizationId),
            BuildStrategicMapTemplate(organizationId),
            BuildAnnualStrategicPlanningTemplate(organizationId),
            BuildOkrTemplate(organizationId),
            BuildPdiTemplate(organizationId)
        };

        var existingTemplateNames = await context.Templates
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId)
            .Select(t => t.Name)
            .ToHashSetAsync();

        var missingTemplates = templates
            .Where(template => !existingTemplateNames.Contains(template.Name))
            .ToList();

        if (missingTemplates.Count > 0)
        {
            context.Templates.AddRange(missingTemplates);
            await context.SaveChangesAsync();
        }
    }

    private static Template BuildBscTemplate(Guid organizationId)
    {
        var financeiraMissionId = Guid.NewGuid();
        var clientesMissionId = Guid.NewGuid();
        var processosMissionId = Guid.NewGuid();
        var aprendizadoMissionId = Guid.NewGuid();

        return new Template
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "BSC",
            Description = "Balanced Scorecard — framework para equilibrar execução estratégica entre finanças, clientes, processos e aprendizado.",
            MissionNamePattern = "BSC — ",
            MissionDescriptionPattern = "Meta estratégica baseada nas perspectivas do Balanced Scorecard.",
            Missions =
            [
                new TemplateMission
                {
                    Id = financeiraMissionId,
                    OrganizationId = organizationId,
                    Name = "Perspectiva Financeira",
                    Description = "Objetivos de desempenho econômico e sustentabilidade financeira.",
                    OrderIndex = 0,
                    Dimension = "Financeira"
                },
                new TemplateMission
                {
                    Id = clientesMissionId,
                    OrganizationId = organizationId,
                    Name = "Perspectiva de Clientes",
                    Description = "Objetivos relacionados à proposta de valor e satisfação do cliente.",
                    OrderIndex = 1,
                    Dimension = "Clientes"
                },
                new TemplateMission
                {
                    Id = processosMissionId,
                    OrganizationId = organizationId,
                    Name = "Perspectiva de Processos Internos",
                    Description = "Objetivos de eficiência e excelência operacional.",
                    OrderIndex = 2,
                    Dimension = "Processos Internos"
                },
                new TemplateMission
                {
                    Id = aprendizadoMissionId,
                    OrganizationId = organizationId,
                    Name = "Perspectiva de Aprendizado e Crescimento",
                    Description = "Objetivos de capacidade organizacional, pessoas e inovação.",
                    OrderIndex = 3,
                    Dimension = "Aprendizado e Crescimento"
                }
            ],
            Indicators =
            [
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Resultado Financeiro",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 0,
                    TemplateMissionId = financeiraMissionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Satisfação de Clientes",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 1,
                    TemplateMissionId = clientesMissionId,
                    QuantitativeType = QuantitativeIndicatorType.KeepAbove,
                    MinValue = 70,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Points
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Eficiência de Processos Internos",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 2,
                    TemplateMissionId = processosMissionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Capacitação e Aprendizado",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 3,
                    TemplateMissionId = aprendizadoMissionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                }
            ]
        };
    }

    private static Template BuildStrategicMapTemplate(Guid organizationId)
    {
        var crescimentoMissionId = Guid.NewGuid();
        var processosMissionId = Guid.NewGuid();
        var clientesMissionId = Guid.NewGuid();
        var financeiraMissionId = Guid.NewGuid();

        return new Template
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "Mapa Estratégico",
            Description = "Mapa Estratégico — template para explicitar objetivos estratégicos e relações de causa e efeito.",
            MissionNamePattern = "Mapa Estratégico — ",
            MissionDescriptionPattern = "Meta para construção e acompanhamento do mapa estratégico.",
            Missions =
            [
                new TemplateMission
                {
                    Id = crescimentoMissionId,
                    OrganizationId = organizationId,
                    Name = "Capacidades Organizacionais",
                    Description = "Base de pessoas, cultura e inovação que viabiliza a estratégia.",
                    OrderIndex = 0,
                    Dimension = "Aprendizado e Crescimento"
                },
                new TemplateMission
                {
                    Id = processosMissionId,
                    OrganizationId = organizationId,
                    Name = "Excelência de Processos",
                    Description = "Processos críticos para entregar valor com previsibilidade.",
                    OrderIndex = 1,
                    Dimension = "Processos Internos"
                },
                new TemplateMission
                {
                    Id = clientesMissionId,
                    OrganizationId = organizationId,
                    Name = "Valor para Clientes",
                    Description = "Resultados percebidos pelos clientes e posicionamento competitivo.",
                    OrderIndex = 2,
                    Dimension = "Clientes"
                },
                new TemplateMission
                {
                    Id = financeiraMissionId,
                    OrganizationId = organizationId,
                    Name = "Resultados Financeiros",
                    Description = "Impacto econômico final esperado da estratégia.",
                    OrderIndex = 3,
                    Dimension = "Financeira"
                }
            ],
            Indicators =
            [
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Objetivo Estratégico 1",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 0,
                    TemplateMissionId = crescimentoMissionId,
                    TargetText = "Descreva o objetivo e as relações de causa e efeito."
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Objetivo Estratégico 2",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 1,
                    TemplateMissionId = processosMissionId,
                    TargetText = "Descreva o objetivo e as relações de causa e efeito."
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Objetivo Estratégico 3",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 2,
                    TemplateMissionId = clientesMissionId,
                    TargetText = "Descreva o objetivo e as relações de causa e efeito."
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Objetivo Estratégico 4",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 3,
                    TemplateMissionId = financeiraMissionId,
                    TargetText = "Descreva o objetivo e as relações de causa e efeito."
                }
            ]
        };
    }

    private static Template BuildAnnualStrategicPlanningTemplate(Guid organizationId)
    {
        var portfolioMissionId = Guid.NewGuid();
        var executionMissionId = Guid.NewGuid();
        var productsMissionId = Guid.NewGuid();

        return new Template
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "Planejamento Estratégico Anual",
            Description = "Template para consolidar prioridades, entregas e marcos estratégicos de um ciclo anual.",
            MissionNamePattern = "Plano Estratégico Anual — ",
            MissionDescriptionPattern = "Planejamento estratégico anual com marcos e prioridades do ciclo.",
            Missions =
            [
                new TemplateMission
                {
                    Id = portfolioMissionId,
                    OrganizationId = organizationId,
                    Name = "Priorização Estratégica",
                    Description = "Definição das frentes prioritárias do ano.",
                    OrderIndex = 0,
                    Dimension = "Financeira"
                },
                new TemplateMission
                {
                    Id = executionMissionId,
                    OrganizationId = organizationId,
                    Name = "Execução e Governança",
                    Description = "Ritmo e disciplina de execução do plano.",
                    OrderIndex = 1,
                    Dimension = "Processos Internos"
                },
                new TemplateMission
                {
                    Id = productsMissionId,
                    OrganizationId = organizationId,
                    Name = "Evolução de Produtos",
                    Description = "Resultados estratégicos esperados para produtos no ciclo.",
                    OrderIndex = 2,
                    Dimension = "Produtos"
                }
            ],
            Indicators =
            [
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Prioridade Estratégica 1",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 0,
                    TemplateMissionId = portfolioMissionId,
                    TargetText = "Descreva o objetivo e os entregáveis da prioridade."
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Prioridade Estratégica 2",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 1,
                    TemplateMissionId = productsMissionId,
                    TargetText = "Descreva o objetivo e os entregáveis da prioridade."
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Execução do Plano no Ano",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 2,
                    TemplateMissionId = executionMissionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                }
            ]
        };
    }

    private static Template BuildOkrTemplate(Guid organizationId)
    {
        var missionId = Guid.NewGuid();

        return new Template
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "OKR",
            Description = "Objectives and Key Results — framework para definir e acompanhar objetivos com resultados-chave mensuráveis.",
            MissionNamePattern = "OKR — ",
            MissionDescriptionPattern = "Meta seguindo o framework OKR com resultados-chave quantitativos.",
            Missions =
            [
                new TemplateMission
                {
                    Id = missionId,
                    OrganizationId = organizationId,
                    Name = "Objetivo Principal",
                    Description = "Objetivo aspiracional do ciclo de OKR.",
                    OrderIndex = 0,
                    Dimension = "Clientes"
                }
            ],
            Indicators =
            [
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Resultado-chave 1",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 0,
                    TemplateMissionId = missionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Resultado-chave 2",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 1,
                    TemplateMissionId = missionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Resultado-chave 3",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 2,
                    TemplateMissionId = missionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                }
            ]
        };
    }

    private static Template BuildPdiTemplate(Guid organizationId)
    {
        var missionId = Guid.NewGuid();

        return new Template
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "PDI",
            Description = "Plano de Desenvolvimento Individual — framework para acompanhar ações de desenvolvimento pessoal e profissional.",
            MissionNamePattern = "PDI — ",
            MissionDescriptionPattern = "Plano de desenvolvimento individual com ações qualitativas e acompanhamento de progresso.",
            Missions =
            [
                new TemplateMission
                {
                    Id = missionId,
                    OrganizationId = organizationId,
                    Name = "Desenvolvimento Individual",
                    Description = "Capacidades e competências a desenvolver no ciclo.",
                    OrderIndex = 0,
                    Dimension = "Aprendizado e Crescimento"
                }
            ],
            Indicators =
            [
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Ação de desenvolvimento 1",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 0,
                    TemplateMissionId = missionId,
                    TargetText = "Descreva a ação de desenvolvimento"
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Ação de desenvolvimento 2",
                    Type = IndicatorType.Qualitative,
                    OrderIndex = 1,
                    TemplateMissionId = missionId,
                    TargetText = "Descreva a ação de desenvolvimento"
                },
                new TemplateIndicator
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Name = "Progresso geral",
                    Type = IndicatorType.Quantitative,
                    OrderIndex = 2,
                    TemplateMissionId = missionId,
                    QuantitativeType = QuantitativeIndicatorType.Achieve,
                    MaxValue = 100,
                    Unit = IndicatorUnit.Percentage
                }
            ]
        };
    }
}
