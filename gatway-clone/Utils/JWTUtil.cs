using Microsoft.IdentityModel.Tokens;

namespace gatway_clone.Utils;

public class JWTUtil
{
    public static readonly TokenValidationParameters ValidationTokenParams = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //IssuerSigningKey = Configuration.JWTSigningCredential.Key,
        //TokenDecryptionKey = Configuration.JWTEncryptingCredentials.Key,
        ValidIssuer = Configuration.JWTIssuer,
        ValidAudience = Configuration.JWTAudience,
        ClockSkew = TimeSpan.Zero
    };
}
