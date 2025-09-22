﻿namespace CleanArchitectureBase.Application.Common.Settings;

public class JwtSettings
{
    public string? SecretKey { get; set; }
    public string? Issuer { get; set; } 
    public string? Audience { get; set; } 
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; } 
}
