namespace Bud.Application.Features.Organizations;

internal static class OrganizationProtectionPolicy
{
    public static bool IsProtectedOrganization(
        OrganizationDomainName organizationName,
        OrganizationDomainName? globalAdminOrganizationName)
    {
        return globalAdminOrganizationName.HasValue && organizationName == globalAdminOrganizationName.Value;
    }
}
