using gatway_clone.Caches;
using gatway_clone.Objects;
using gatway_clone.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
namespace gatway_clone.Controllers.Auth;

public class HandshakeController : AuthController<HandshakeRequest, HandshakeResponse>
{
    protected override async Task<ResponseDTO<HandshakeResponse>> Process()
    {
        SessionObject sessionObject = new()
        {
            Key = RequestDTO.RequestID,
            SessionKey = CryptoUtil.GenerateKey(),
            Salt = LogUtil.GenerateLMID()
        };
        AsymmetricCipherKeyPair keyPair = CryptoUtil.ECDH.GenerateKeyPair("secp256r1");
        byte[] sharedKey = CryptoUtil.ECDH.GetKeyShared(keyPair, Hex.Decode(RequestData.PublicKey));
        ResponseData.SessionKey = Hex.ToHexString(CryptoUtil.Encrypt(sharedKey, sessionObject.SessionKey));
        ResponseData.Salt = CryptoUtil.Encrypt(sharedKey, sessionObject.Salt);
        ResponseData.PublicKey = Hex.ToHexString(keyPair.GetPublicKey());
        //await SessionKeyCache.Instance.AddAsync(RequestDTO.LMID, sessionObject);
        return await BuildResponse(ResponseStatus.Success);
        
    }
}
