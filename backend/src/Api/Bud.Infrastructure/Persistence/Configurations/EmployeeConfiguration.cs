using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(c => c.FullName)
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .HasMaxLength(320);

        builder.Property(c => c.Nickname)
            .HasMaxLength(100);

        builder.Property(c => c.Language)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.Email)
            .IsUnique();
    }
}
