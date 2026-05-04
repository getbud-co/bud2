namespace Bud.Application.Tests.Sessions;

public sealed class CreateSessionTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAuthSucceeds_ShouldReturnSessionResponse()
    {
        var loginResult = new LoginResult
        {
            Token = "jwt-token",
            Email = "user@bud.co",
            DisplayName = "User Bud",
            IsGlobalAdmin = false
        };
        var authenticator = new Mock<ISessionAuthenticator>();
        authenticator.Setup(x => x.LoginAsync(It.IsAny<CreateSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResult>.Success(loginResult));

        var sut = new CreateSession(authenticator.Object, NullLogger<CreateSession>.Instance);

        var result = await sut.ExecuteAsync(new CreateSessionRequest { Email = "user@bud.co" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.Email.Should().Be("user@bud.co");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthFails_ShouldReturnFailure()
    {
        var authenticator = new Mock<ISessionAuthenticator>();
        authenticator.Setup(x => x.LoginAsync(It.IsAny<CreateSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResult>.Failure("Credenciais inválidas.", ErrorType.Unauthorized));

        var sut = new CreateSession(authenticator.Object, NullLogger<CreateSession>.Instance);

        var result = await sut.ExecuteAsync(new CreateSessionRequest { Email = "wrong@bud.co" });

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
    }
}
