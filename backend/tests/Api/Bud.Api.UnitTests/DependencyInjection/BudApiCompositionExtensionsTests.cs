using Bud.Api.DependencyInjection;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bud.Api.UnitTests.DependencyInjection;

public sealed class BudApiCompositionExtensionsTests
{
    [Fact]
    public void AddBudApi_WithConfiguredLocalhostOrigin_AddsCommonLocalDevelopmentOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:8080"
            })
            .Build();

        // Act
        services.AddBudApi(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy("LocalDevelopmentClient");

        // Assert
        policy.Should().NotBeNull();
        policy!.Origins.Should().BeEquivalentTo(
            "http://localhost:8080",
            "http://127.0.0.1:8080",
            "http://0.0.0.0:8080");
    }

    [Fact]
    public void AddBudApi_WithCustomOrigins_PreservesConfiguredOriginsWithoutDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://bud.local",
                ["Cors:AllowedOrigins:1"] = "http://127.0.0.1:8080"
            })
            .Build();

        // Act
        services.AddBudApi(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy("LocalDevelopmentClient");

        // Assert
        policy.Should().NotBeNull();
        policy!.Origins.Should().OnlyHaveUniqueItems();
        policy.Origins.Should().Contain("https://bud.local");
        policy.Origins.Should().Contain("http://localhost:8080");
        policy.Origins.Should().Contain("http://127.0.0.1:8080");
        policy.Origins.Should().Contain("http://0.0.0.0:8080");
    }
}
