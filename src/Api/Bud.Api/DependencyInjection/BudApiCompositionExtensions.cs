using System.Reflection;
using Bud.Api.Middleware;
using Bud.Api.Serialization;
using Bud.Api.Settings;
using Bud.Application.Configuration;
using Bud.Api.Features.Organizations;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;

namespace Bud.Api.DependencyInjection;

public static class BudApiCompositionExtensions
{
    internal const string LocalDevelopmentCorsPolicy = "LocalDevelopmentClient";
    private static readonly string[] CommonLocalDevelopmentOrigins =
    [
        "http://localhost:8080",
        "http://127.0.0.1:8080",
        "http://0.0.0.0:8080"
    ];

    public static IServiceCollection AddBudApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Converters.Add(new LenientEnumJsonConverterFactory());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });

        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddValidatorsFromAssemblyContaining<CreateOrganizationValidator>();
        services.AddCors(options =>
        {
            options.AddPolicy(LocalDevelopmentCorsPolicy, policy =>
            {
                var allowedOrigins = ResolveLocalDevelopmentCorsOrigins(configuration);

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static string[] ResolveLocalDevelopmentCorsOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            ?? [];

        return configuredOrigins
            .Concat(CommonLocalDevelopmentOrigins)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IServiceCollection AddBudSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GlobalAdminSettings>(configuration.GetSection("GlobalAdminSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<RateLimitSettings>(configuration.GetSection("RateLimitSettings"));
        return services;
    }
}
