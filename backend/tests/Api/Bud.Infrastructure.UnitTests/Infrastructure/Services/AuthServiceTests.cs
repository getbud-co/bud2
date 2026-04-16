using Bud.Infrastructure.Persistence;
using Bud.Application.Configuration;
using Bud.Infrastructure.UnitTests.Helpers;
using Bud.Shared.Contracts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;
using Bud.Application.Common;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public class AuthServiceTests
{
    private readonly TestTenantProvider _tenantProvider = new() { IsGlobalAdmin = true };

    private static readonly JwtSettings TestJwtSettings = new()
    {
        Key = "test-secret-key-for-unit-tests-minimum-32-characters",
        Issuer = "bud-test",
        Audience = "bud-test-api"
    };

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, _tenantProvider);
    }

    private static SessionAuthenticator CreateService(ApplicationDbContext context)
    {
        return new SessionAuthenticator(context, Options.Create(TestJwtSettings));
    }

    private static MyOrganizationsReadStore CreateReadStore(ApplicationDbContext context)
    {
        return new MyOrganizationsReadStore(context);
    }

    #region Global Admin Login Detection Tests

    [Theory]
    [InlineData("admin@getbud.co")]
    [InlineData("ADMIN@GETBUD.CO")]
    [InlineData("Admin@GetBud.Co")]
    public async Task Login_WithGlobalAdminEmail_ReturnsGlobalAdminUser(string email)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var org = new Organization { Id = Guid.NewGuid(), Name = "Admin Org" };
        context.Organizations.Add(org);
        var adminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador Global",
            Email = "admin@getbud.co",
        };
        context.Employees.Add(adminEmployee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = adminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = new CreateSessionRequest { Email = email };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsGlobalAdmin.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Administrador Global");
        result.Value!.Email.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("admin@company.com")]
    [InlineData("admin@test")]
    public async Task Login_WithNonExistentEmail_ReturnsNotFound(string email)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        var request = new CreateSessionRequest { Email = email };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    #endregion

    #region Employee Login Tests

    [Fact]
    public async Task Login_WithExistingEmployee_ReturnsEmployeeData()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Test Team",
            OrganizationId = org.Id,
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            Email = "john.doe@example.com",
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = new CreateSessionRequest { Email = "john.doe@example.com" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsGlobalAdmin.Should().BeFalse();
        result.Value!.Email.Should().Be("john.doe@example.com");
        result.Value!.DisplayName.Should().Be("John Doe");
        result.Value!.EmployeeId.Should().Be(employee.Id);
        result.Value!.Role.Should().Be(EmployeeRole.Contributor);
        result.Value!.OrganizationId.Should().Be(org.Id);
    }

    [Fact]
    public async Task Login_WithExistingEmployee_RegistersAccessLog()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", OrganizationId = org.Id };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Doe",
            Email = "jane.doe@example.com",
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LoginAsync(new CreateSessionRequest { Email = employee.Email });

        // Assert
        result.IsSuccess.Should().BeTrue();
        var accessLogs = await context.EmployeeAccessLogs.ToListAsync();
        accessLogs.Should().ContainSingle();
        accessLogs[0].EmployeeId.Should().Be(employee.Id);
        accessLogs[0].OrganizationId.Should().Be(org.Id);
        accessLogs[0].AccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNotFoundMessage()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        var request = new CreateSessionRequest { Email = "nonexistent@example.com" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Usuário não encontrado.");
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsFailure()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        var request = new CreateSessionRequest { Email = "" };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("Informe o e-mail.");
    }

    [Fact]
    public async Task Login_WithWhitespaceEmail_ReturnsFailure()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateService(context);

        var request = new CreateSessionRequest { Email = "   " };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("Informe o e-mail.");
    }

    #endregion

    #region Email Normalization Tests

    [Theory]
    [InlineData("Test@Example.Com", "test@example.com")]
    [InlineData("USER@DOMAIN.COM", "user@domain.com")]
    [InlineData("MixedCase@Email.Net", "mixedcase@email.net")]
    public async Task Login_WithUpperCaseEmail_NormalizesToLowerCase(string inputEmail, string expectedEmail)
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Test Team",
            OrganizationId = org.Id
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = expectedEmail,
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = new CreateSessionRequest { Email = inputEmail };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(expectedEmail);
        result.Value!.EmployeeId.Should().Be(employee.Id);
    }

    #endregion

    #region Global Admin with Employee Tests

    [Fact]
    public async Task Login_WithGlobalAdminEmailAndExistingEmployee_ReturnsGlobalAdminWithEmployeeData()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var globalAdminEmail = "admin@getbud.co";

        var org = new Organization { Id = Guid.NewGuid(), Name = "Admin Org" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Admin Team", OrganizationId = org.Id };
        var adminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Admin Employee",
            Email = globalAdminEmail,
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.Add(adminEmployee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = adminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = new CreateSessionRequest { Email = globalAdminEmail };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsGlobalAdmin.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(adminEmployee.Id);
        result.Value!.OrganizationId.Should().Be(org.Id);
        result.Value!.DisplayName.Should().Be("Admin Employee");
    }

    [Fact]
    public async Task Login_WithGlobalAdminEmailAndExistingEmployee_RegistersAccessLog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var globalAdminEmail = "admin@getbud.co";

        var org = new Organization { Id = Guid.NewGuid(), Name = "Admin Org" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Admin Team", OrganizationId = org.Id };
        var adminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Admin Employee",
            Email = globalAdminEmail,
        };

        context.Organizations.Add(org);
        context.Teams.Add(team);
        context.Employees.Add(adminEmployee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = adminEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LoginAsync(new CreateSessionRequest { Email = globalAdminEmail });

        // Assert
        result.IsSuccess.Should().BeTrue();
        var accessLogs = await context.EmployeeAccessLogs.ToListAsync();
        accessLogs.Should().ContainSingle();
        accessLogs[0].EmployeeId.Should().Be(adminEmployee.Id);
        accessLogs[0].OrganizationId.Should().Be(org.Id);
    }

    #endregion

    #region IsGlobalAdmin Database Flag Tests

    [Fact]
    public async Task Login_WithIsGlobalAdminTrue_ReturnsGlobalAdmin()
    {
        // Arrange - employee with arbitrary email but IsGlobalAdmin = true
        using var context = CreateInMemoryContext();

        var org = new Organization { Id = Guid.NewGuid(), Name = "Any Org" };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Custom Admin",
            Email = "custom.admin@anycompany.com",
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LoginAsync(new CreateSessionRequest { Email = "custom.admin@anycompany.com" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGlobalAdmin.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Custom Admin");
        result.Value!.EmployeeId.Should().Be(employee.Id);
    }

    [Fact]
    public async Task Login_WithIsGlobalAdminFalse_AndAdminEmail_ReturnsRegularUser()
    {
        // Arrange - employee with admin@getbud.co email but IsGlobalAdmin = false
        // Proves that the email alone no longer grants admin privileges
        using var context = CreateInMemoryContext();

        var org = new Organization { Id = Guid.NewGuid(), Name = "Test Org" };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Not Actually Admin",
            Email = "admin@getbud.co",
        };

        context.Organizations.Add(org);
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = false
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LoginAsync(new CreateSessionRequest { Email = "admin@getbud.co" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGlobalAdmin.Should().BeFalse();
        result.Value!.DisplayName.Should().Be("Not Actually Admin");
    }

    #endregion

    #region GetMyOrganizations Tests

    [Fact]
    public async Task GetMyOrganizations_WithIsGlobalAdminEmployee_ReturnsAllOrganizations()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
        var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
        var org3 = new Organization { Id = Guid.NewGuid(), Name = "Org 3" };
        context.Organizations.AddRange(org1, org2, org3);

        var adminEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Global Admin",
            Email = "globaladmin@anycompany.com",
        };
        context.Employees.Add(adminEmployee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = adminEmployee.Id,
            OrganizationId = org1.Id,
            Role = EmployeeRole.TeamLeader,
            IsGlobalAdmin = true
        });
        await context.SaveChangesAsync();

        var service = CreateReadStore(context);

        // Act
        var result = await service.GetMyOrganizationsAsync("globaladmin@anycompany.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value!.Select(o => o.Name).Should().Contain(["Org 1", "Org 2", "Org 3"]);
    }

    [Fact]
    public async Task GetMyOrganizations_WithRegularUser_ReturnsOnlyMemberOrganizations()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var userEmail = "user@example.com";

        var org1 = new Organization { Id = Guid.NewGuid(), Name = "User Org" };
        var org2 = new Organization { Id = Guid.NewGuid(), Name = "Other Org" };
        context.Organizations.AddRange(org1, org2);

        var team = new Team { Id = Guid.NewGuid(), Name = "Team", OrganizationId = org1.Id};
        context.Teams.Add(team);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = userEmail,
        };
        context.Employees.Add(employee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = employee.Id,
            OrganizationId = org1.Id,
            Role = EmployeeRole.Contributor,
        });
        await context.SaveChangesAsync();

        var service = CreateReadStore(context);

        // Act
        var result = await service.GetMyOrganizationsAsync(userEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value![0].Id.Should().Be(org1.Id);
        result.Value[0].Name.Should().Be("User Org");
    }

    [Fact]
    public async Task GetMyOrganizations_WithEmployee_ReturnsOrganizations()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var ownerEmail = "employee@example.com";

        var org = new Organization { Id = Guid.NewGuid(), Name = "Owned Org" };
        context.Organizations.Add(org);

        var team = new Team { Id = Guid.NewGuid(), Name = "Team", OrganizationId = org.Id };
        context.Teams.Add(team);

        var ownerEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Employee",
            Email = ownerEmail,
        };
        context.Employees.Add(ownerEmployee);
        context.Memberships.Add(new Membership
        {
            EmployeeId = ownerEmployee.Id,
            OrganizationId = org.Id,
            Role = EmployeeRole.Contributor,
        });
        await context.SaveChangesAsync();

        var service = CreateReadStore(context);

        // Act
        var result = await service.GetMyOrganizationsAsync(ownerEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value![0].Id.Should().Be(org.Id);
    }

    [Fact]
    public async Task GetMyOrganizations_WithEmptyEmail_ReturnsFailure()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateReadStore(context);

        // Act
        var result = await service.GetMyOrganizationsAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("E-mail é obrigatório.");
    }

    [Fact]
    public async Task GetMyOrganizations_WithWhitespaceEmail_ReturnsFailure()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = CreateReadStore(context);

        // Act
        var result = await service.GetMyOrganizationsAsync("   ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("E-mail é obrigatório.");
    }

    #endregion
}
