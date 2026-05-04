namespace Bud.Domain.ValueObjects;

public readonly record struct EmployeeName
{
    private const int MaxLength = 200;

    private EmployeeName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(string? raw, out EmployeeName employeeName)
    {
        employeeName = default;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalized = string.Join(' ', raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length is < 2 or > MaxLength)
        {
            return false;
        }

        employeeName = new EmployeeName(normalized);
        return true;
    }

    public static EmployeeName Create(string? raw)
    {
        if (!TryCreate(raw, out var employeeName))
        {
            throw new DomainInvariantException("O nome do colaborador é obrigatório.");
        }

        return employeeName;
    }

    public override string ToString() => Value;
}
