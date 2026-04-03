namespace InsurancePlatform.Application.Features.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    bool MfaRequired
);
