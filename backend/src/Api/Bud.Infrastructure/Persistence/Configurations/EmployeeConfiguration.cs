using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(c => c.FullName)
            .HasConversion(
                fullName => fullName.Value,
                value => EmployeeName.Create(value))
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(320);

        builder.Property(c => c.IsGlobalAdmin)
            .HasDefaultValue(false);

        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.OrganizationId);

        builder.HasIndex(c => c.Email)
            .IsUnique();
    }
}
