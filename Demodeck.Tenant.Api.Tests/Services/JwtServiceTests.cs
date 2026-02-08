using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Demodeck.Tenant.Api.Models;
using Demodeck.Tenant.Api.Services;
using Microsoft.IdentityModel.Tokens;

namespace Demodeck.Tenant.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly Manager _testManager;

    public JwtServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256Algorithm!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            TokenLifetimeMinutes = 60
        };
        _jwtService = new JwtService(_jwtSettings);
        _testManager = new Manager
        {
            Id = "mgr_test",
            Username = "testuser",
            Email = "test@example.com",
            Role = "Admin"
        };
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var token = _jwtService.GenerateToken(_testManager);
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtFormat()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_ContainsNameIdentifierClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
            || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        Assert.NotNull(claim);
        Assert.Equal("mgr_test", claim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsNameClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name
            || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        Assert.NotNull(claim);
        Assert.Equal("testuser", claim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email
            || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        Assert.NotNull(claim);
        Assert.Equal("test@example.com", claim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role
            || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        Assert.NotNull(claim);
        Assert.Equal("Admin", claim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsJtiClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == "jti");
        Assert.NotNull(jti);
        Assert.True(Guid.TryParse(jti.Value, out _));
    }

    [Fact]
    public void GenerateToken_ContainsIatClaim()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var iat = jwt.Claims.FirstOrDefault(c => c.Type == "iat");
        Assert.NotNull(iat);
        Assert.True(long.TryParse(iat.Value, out var timestamp));
        Assert.True(timestamp > 0);
    }

    [Fact]
    public void GenerateToken_UsesCorrectIssuer()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("TestIssuer", jwt.Issuer);
    }

    [Fact]
    public void GenerateToken_UsesCorrectAudience()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains("TestAudience", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresAtExpectedTime()
    {
        var before = DateTime.UtcNow;
        var token = _jwtService.GenerateToken(_testManager);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = before.AddMinutes(_jwtSettings.TokenLifetimeMinutes);
        Assert.NotNull(jwt.ValidTo);
        // Allow 5 seconds tolerance
        Assert.InRange(jwt.ValidTo, expectedExpiry.AddSeconds(-5), expectedExpiry.AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_CanBeValidatedWithSameKey()
    {
        var token = _jwtService.GenerateToken(_testManager);
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = true
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
    }
}
