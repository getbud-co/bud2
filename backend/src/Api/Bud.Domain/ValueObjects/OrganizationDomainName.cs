using System.Text.RegularExpressions;

namespace Bud.Domain.ValueObjects;

public readonly partial record struct OrganizationDomainName
{
    private const int MaxLength = 200;

    private OrganizationDomainName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(string? raw, out OrganizationDomainName organizationDomainName)
    {
        organizationDomainName = default;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalized = raw.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength)
        {
            return false;
        }

        if (!DomainPattern().IsMatch(normalized))
        {
            return false;
        }

        organizationDomainName = new OrganizationDomainName(normalized);
        return true;
    }

    public static OrganizationDomainName Create(string? raw)
    {
        if (!TryCreate(raw, out var organizationDomainName))
        {
            throw new DomainInvariantException("O nome da organização deve ser um domínio válido (ex: empresa.com.br).");
        }

        return organizationDomainName;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", RegexOptions.CultureInvariant)]
    private static partial Regex DomainPattern();
}
