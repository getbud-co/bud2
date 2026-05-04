namespace Bud.Application.Tests.Sessions;

public sealed class DeleteCurrentSessionTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldAlwaysSucceed()
    {
        var sut = new DeleteCurrentSession(NullLogger<DeleteCurrentSession>.Instance);

        var result = await sut.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
    }
}
