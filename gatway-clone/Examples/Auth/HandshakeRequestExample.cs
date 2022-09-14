using gatway_clone.Objects;
using gatway_clone.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;

namespace gatway_clone.Examples.Auth;

public  class HandshakeRequestExample : ExampleBase<HandshakeRequest, HandshakeResponse>
{
    public override IEnumerable<SwaggerExample<RequestDTO<HandshakeRequest>>> GetSwaggerExamples()
    {
        yield return CreateExample("Success", new()
        {
            PublicKey = Hex.ToHexString(CryptoUtil.ECDH.GenerateKeyPair("secp256r1").GetPublicKey())
        });
    }
    public class HandshakeResponseExample : ExampleBase<HandshakeResponse>
    {
        public override IEnumerable<SwaggerExample<ResponseDTO<HandshakeResponse>>> GetSwaggerExamples()
        {
            AsymmetricCipherKeyPair keyPair = CryptoUtil.ECDH.GenerateKeyPair("secp256r1");
            byte[] keyShared = CryptoUtil.ECDH.GetKeyShared(keyPair, CryptoUtil.ECDH.GenerateKeyPair("secp256r1").GetPublicKey());
            yield return CreateExample("Success", ResponseStatus.Success, new()
            {
                PublicKey = Hex.ToHexString(keyPair.GetPublicKey()),
                SessionKey = Hex.ToHexString(CryptoUtil.Encrypt(keyShared, CryptoUtil.GenerateKey())),
                Salt = CryptoUtil.Encrypt(keyShared, LogUtil.GenerateLMID())
            });
        }
    }
}
