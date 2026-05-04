using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bud.Application.Common;
using Bud.Application.Configuration;
using Bud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bud.Infrastructure.Features.Sessions;

public sealed class SessionAuthenticator(
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtOptions) : ISessionAuthenticator
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<LoginResult>> LoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (!EmailAddress.TryCreate(request.Email, out var emailAddress))
        {
            return Result<LoginResult>.Failure("Informe o e-mail.");
        }

        var employee = await dbContext.Employees
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => EF.Property<string>(c, nameof(Employee.Email)) == emailAddress.Value, cancellationToken);

        if (employee is null)
        {
            return Result<LoginResult>.Unauthorized("Falha ao autenticar.");
        }

        // Load the membership for the organization context
        var member = await dbContext.Memberships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.EmployeeId == employee.Id, cancellationToken);

        if (member is null)
        {
            return Result<LoginResult>.NotFound("Usuário não encontrado.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, employee.Email.Value),
            new("email", employee.Email.Value),
            new("employee_id", employee.Id.ToString()),
            new("organization_id", member.OrganizationId.ToString()),
            new(ClaimTypes.Name, employee.FullName),
        };

        if (member.IsGlobalAdmin)
        {
            claims.Add(new(ClaimTypes.Role, "GlobalAdmin"));
        }

        await RegisterAccessLogAsync(employee.Id, member.OrganizationId, cancellationToken);

        var token = GenerateJwtToken(claims);

        return Result<LoginResult>.Success(new LoginResult
        {
            Token = token,
            Email = employee.Email,
            DisplayName = employee.FullName,
            IsGlobalAdmin = member.IsGlobalAdmin,
            EmployeeId = employee.Id,
            Role = member.Role,
            OrganizationId = member.OrganizationId,
        });
    }

    private string GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.TokenExpirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
