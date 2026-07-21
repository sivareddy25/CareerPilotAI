using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication;
using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Issues HS256-signed access tokens.
/// </summary>
/// <remarks>
/// <para>
/// Symmetric signing is appropriate because the same application both issues and
/// validates these tokens. If a third party ever needs to validate them without being
/// able to mint them, this must move to RS256 — with HS256 the validation key *is* the
/// signing key, so handing it out hands out the ability to forge.
/// </para>
/// <para>
/// Roles and permissions are embedded as claims so that the common case — checking
/// authorization on a request — costs no database round-trip. The trade-off is
/// staleness: a privilege revoked mid-token stays in the bearer's claims until it
/// expires. That window is bounded by the short access-token lifetime and closed at
/// refresh, where state is re-read.
/// </para>
/// </remarks>
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;

        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);

        // HS256 keys shorter than the 256-bit hash output weaken the signature, and
        // the library will reject them anyway. Failing at startup with a clear reason
        // beats failing on the first login attempt with an obscure one.
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be at least 32 bytes (256 bits) for HS256. " +
                "Supply it from a secret store, not from appsettings.json.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(permissions);

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);
        var tokenId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            // jti is what sign-out blacklists; without a unique one per token there
            // would be no way to revoke a single session.
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(issuedAt).ToString(),
                ClaimValueTypes.Integer64),
            new(AuthenticationClaimTypes.SecurityStamp, user.SecurityStamp),
        };

        claims.AddRange(roles.Select(role => new Claim(AuthenticationClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(p => new Claim(AuthenticationClaimTypes.Permission, p)));

        // Nothing sensitive goes in: a JWT payload is base64, not encrypted, and any
        // holder can read it. Identity and authorization only — never the password
        // hash, the refresh token, or personal data beyond the email already known to
        // the account holder.
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(value, tokenId, expiresAt);
    }
}
