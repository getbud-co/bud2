using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class EmployeeAccessLogConfiguration : IEntityTypeConfiguration<EmployeeAccessLog>
{
    public void Configure(EntityTypeBuilder<EmployeeAccessLog> builder)
    {
        builder.HasOne(cal => cal.Organization)
            .WithMany()
            .HasForeignKey(cal => cal.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cal => cal.Employee)
            .WithMany()
            .HasForeignKey(cal => cal.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cal => new { cal.OrganizationId, cal.AccessedAt });
    }
}
