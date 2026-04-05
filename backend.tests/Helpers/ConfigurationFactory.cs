using Microsoft.Extensions.Configuration;

namespace SampleTech.Api.Tests.Helpers;

public static class ConfigurationFactory
{
    public static IConfiguration CreateJwtConfig(
        string key = "SuperSecretTestKeyThatIsLongEnoughForHS256!",
        string issuer = "test-issuer",
        string audience = "test-audience",
        int accessExpiryMinutes = 60,
        int refreshExpiryDays = 30)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience,
                ["Jwt:AccessTokenExpiryMinutes"] = accessExpiryMinutes.ToString(),
                ["Jwt:RefreshTokenExpiryDays"] = refreshExpiryDays.ToString()
            })
            .Build();
    }
}
