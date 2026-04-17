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

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, employee.Email.Value),
            new("email", employee.Email.Value),
            new("employee_id", employee.Id.ToString()),
            new("organization_id", employee.OrganizationId.ToString()),
            new(ClaimTypes.Name, employee.FullName.Value),
            new(ClaimTypes.Role, employee.Role.ToString())
        };

        if (employee.IsGlobalAdmin)
        {
            claims.Add(new(ClaimTypes.Role, "GlobalAdmin"));
        }

        var token = GenerateJwtToken(claims);

        return Result<LoginResult>.Success(new LoginResult
        {
            Token = token,
            Email = employee.Email.Value,
            DisplayName = employee.FullName.Value,
            IsGlobalAdmin = employee.IsGlobalAdmin,
            EmployeeId = employee.Id,
            Role = employee.Role,
            OrganizationId = employee.OrganizationId
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
