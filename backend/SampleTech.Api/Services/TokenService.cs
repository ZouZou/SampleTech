using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SampleTech.Api.Models;
using JwtClaim = System.Security.Claims.Claim;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace SampleTech.Api.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    private const string TenantIdClaimType = "tenant_id";

    public TokenPair GenerateTokenPair(User user)
    {
        var jwtSection = config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(
            jwtSection.GetValue<int>("AccessTokenExpiryMinutes", 60));

        var claims = new List<JwtClaim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue)
            claims.Add(new JwtClaim(TenantIdClaimType, user.TenantId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiry.UtcDateTime,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenPair(accessToken, refreshToken, expiry);
    }

    public Guid? ValidateRefreshToken(string token)
    {
        // Refresh token validation is handled at the DB layer in UserService.
        _ = token;
        return null;
    }
}
