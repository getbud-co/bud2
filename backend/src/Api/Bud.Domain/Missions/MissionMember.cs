namespace Bud.Domain.Missions;

public sealed class MissionMember
{
    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public MissionMemberRole Role { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public Guid? AddedById { get; set; }
    public Employee? AddedBy { get; set; }

    public static MissionMember Create(
        Guid missionId,
        Guid employeeId,
        MissionMemberRole role,
        Guid? addedById = null)
    {
        return new MissionMember
        {
            MissionId = missionId,
            EmployeeId = employeeId,
            Role = role,
            AddedById = addedById,
            AddedAt = DateTime.UtcNow
        };
    }

    public void UpdateRole(MissionMemberRole role)
    {
        Role = role;
    }
}
