using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(o => o.Name)
            .HasConversion(
                name => name.Value,
                value => OrganizationDomainName.Create(value))
            .HasMaxLength(200);

        builder.Property(o => o.Cnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(o => o.Plan)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.ContractStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.IconUrl)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .IsRequired();


    }
}
