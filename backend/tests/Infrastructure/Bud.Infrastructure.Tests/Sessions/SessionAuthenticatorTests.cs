namespace Bud.Infrastructure.Tests.Sessions;

/// <summary>
/// SessionAuthenticator uses EF.Property&lt;string&gt; with value-converted properties
/// for email lookup, which is Npgsql-specific. These tests require a real database.
/// JWT generation is tested indirectly via the application layer tests.
/// </summary>
public sealed class SessionAuthenticatorTests
{
    [Fact]
    public void JwtSettings_ShouldHaveDefaults()
    {
        var settings = new JwtSettings();

        settings.Issuer.Should().Be("bud-dev");
        settings.Audience.Should().Be("bud-api");
        settings.TokenExpirationHours.Should().Be(8);
    }
}
