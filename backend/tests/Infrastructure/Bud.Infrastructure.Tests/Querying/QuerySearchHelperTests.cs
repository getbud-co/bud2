namespace Bud.Infrastructure.Tests.Querying;

public sealed class QuerySearchHelperTests
{
    private static readonly List<string> Data = ["Alice", "Bob", "Charlie", "alice2"];

    [Fact]
    public void ApplyCaseInsensitiveSearch_WithNullSearch_ShouldReturnOriginalQuery()
    {
        var query = Data.AsQueryable();

        var result = QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, null, false,
            (q, _) => throw new InvalidOperationException("Should not be called"),
            (q, _) => throw new InvalidOperationException("Should not be called"));

        result.Should().HaveCount(4);
    }

    [Fact]
    public void ApplyCaseInsensitiveSearch_WithEmptySearch_ShouldReturnOriginalQuery()
    {
        var query = Data.AsQueryable();

        var result = QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, "   ", false,
            (q, _) => throw new InvalidOperationException("Should not be called"),
            (q, _) => throw new InvalidOperationException("Should not be called"));

        result.Should().HaveCount(4);
    }

    [Fact]
    public void ApplyCaseInsensitiveSearch_WhenNotNpgsql_ShouldUseFallbackFilter()
    {
        var query = Data.AsQueryable();
        var usedFallback = false;

        QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, "alice", false,
            (q, _) => throw new InvalidOperationException("Should not be called"),
            (q, term) =>
            {
                usedFallback = true;
                return q.Where(x => x.Contains(term, StringComparison.OrdinalIgnoreCase));
            });

        usedFallback.Should().BeTrue();
    }

    [Fact]
    public void ApplyCaseInsensitiveSearch_WhenNpgsql_ShouldUseNpgsqlFilter()
    {
        var query = Data.AsQueryable();
        var usedNpgsql = false;

        QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, "alice", true,
            (q, pattern) =>
            {
                usedNpgsql = true;
                pattern.Should().Be("%alice%");
                return q;
            },
            (q, _) => throw new InvalidOperationException("Should not be called"));

        usedNpgsql.Should().BeTrue();
    }

    [Fact]
    public void ApplyCaseInsensitiveSearch_ShouldTrimSearchTerm()
    {
        var query = Data.AsQueryable();
        string? capturedTerm = null;

        QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, "  alice  ", false,
            (q, _) => throw new InvalidOperationException("Should not be called"),
            (q, term) =>
            {
                capturedTerm = term;
                return q;
            });

        capturedTerm.Should().Be("alice");
    }

    [Fact]
    public void ApplyCaseInsensitiveSearch_Npgsql_ShouldEscapeSpecialChars()
    {
        var query = Data.AsQueryable();
        string? capturedPattern = null;

        QuerySearchHelper.ApplyCaseInsensitiveSearch(
            query, "100%_done", true,
            (q, pattern) =>
            {
                capturedPattern = pattern;
                return q;
            },
            (q, _) => throw new InvalidOperationException("Should not be called"));

        capturedPattern.Should().Be("%100\\%\\_done%");
    }
}
