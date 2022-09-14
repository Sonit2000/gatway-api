using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using static gatway_clone.Utils.NginxUtil;

namespace gatway_clone;

public class Configuration
{
    //public static readonly EncryptingCredentials JWTEncryptingCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecureConfiguration.DecryptConfiguration("JWT:EncryptKey"))), JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);
    private static readonly IConfiguration _appSettings = new ConfigurationBuilder().AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json")).Build();
    public static readonly int SettlementMaxThread = _appSettings.GetValue<int>("SettlementMaxThread");
    public static readonly TimeSpan DefaultFunctionTimeout = _appSettings.GetValue("DefaultFunctionTimeout", TimeSpan.FromSeconds(30));
    public static readonly TimeSpan MaxDriftTimestamp = _appSettings.GetValue("MaxDriftTimestamp", TimeSpan.FromSeconds(10));
    public static readonly string ApplicationUrl = _appSettings["Cluster:ApplicationUrl"];
    public static readonly int SettlementMaxRetries = _appSettings.GetValue<int>("SettlementMaxRetries");
    public static readonly bool AutoSettlement = _appSettings.GetValue("AutoSettlement", true);
    public static readonly NginxServerInfo NginxPlusCurrentNodeInfo = _appSettings.GetSection("NginxPlus:CurrentNodeInfo").Get<NginxServerInfo>();
    public static readonly string[] NginxPlusUrls = _appSettings.GetSection("NginxPlus:Urls").Get<string[]>();
    public static readonly string JWTAudience = _appSettings["JWT:Audience"];
    public static readonly bool IsUseNginxPlus = _appSettings.GetSection("NginxPlus").Exists();
    //public static readonly SigningCredentials JWTSigningCredential = new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecureConfiguration.DecryptConfiguration("JWT:SignKey"))), SecurityAlgorithms.HmacSha256);
    public static readonly string JWTIssuer = _appSettings["JWT:Issuer"];
    public static readonly bool IsEnableSwagger = _appSettings.GetValue("IsEnableSwagger", true);
    public static readonly string AppName = _appSettings["Cluster:AppName"];

}
