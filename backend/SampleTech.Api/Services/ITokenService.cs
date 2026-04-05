using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiry);

public interface ITokenService
{
    TokenPair GenerateTokenPair(User user);
    Guid? ValidateRefreshToken(string token);
}
