namespace Bud.Domain.Indicators;

public sealed class Checkin : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid IndicatorId { get; set; }
    public Indicator Indicator { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public CheckinConfidence? Confidence { get; set; }
    public string? Note { get; set; }
    public Guid[] Mentions { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static Checkin Create(
        Guid id,
        Guid organizationId,
        Guid indicatorId,
        Guid employeeId,
        decimal value,
        DateTime createdAt,
        string? note,
        int confidenceLevel)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Check-in deve pertencer a uma organização válida.");
        }

        if (indicatorId == Guid.Empty)
        {
            throw new DomainInvariantException("Check-in deve pertencer a um indicador válido.");
        }

        if (employeeId == Guid.Empty)
        {
            throw new DomainInvariantException("Check-in deve ter um colaborador válido.");
        }

        var checkin = new Checkin
        {
            Id = id,
            OrganizationId = organizationId,
            IndicatorId = indicatorId,
            EmployeeId = employeeId,
            PreviousValue = 0,
            CreatedAt = createdAt
        };

        checkin.Update(value, createdAt, note, confidenceLevel);
        return checkin;
    }

    public void Update(decimal value, DateTime createdAt, string? note, int confidenceLevel)
    {
        if (confidenceLevel < 1 || confidenceLevel > 5)
        {
            throw new DomainInvariantException("Nível de confiança deve ser entre 1 e 5.");
        }

        PreviousValue = Value;
        Value = value;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        CreatedAt = createdAt;
    }
}
