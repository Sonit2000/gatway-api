using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace gatway_clone.Utils;

public static class CryptoUtil
{
    public static byte[] GenerateKey(SymmetricAlgorithmEnum algorithm = SymmetricAlgorithmEnum.Aes, int? keySize = null)
    {
        using SymmetricAlgorithm provider = SymmetricAlgorithm.Create(algorithm.ToString());
        if (keySize.HasValue) provider.KeySize = keySize.Value;
        provider.GenerateKey();
        return provider.Key;
    }
    public static class ECDH
    {
        public static AsymmetricCipherKeyPair GenerateKeyPair(string curveName = "secp521r1")
        {
            X9ECParameters ecParameters = SecNamedCurves.GetByName(curveName);
            ECKeyPairGenerator generator = new();
            generator.Init(new ECKeyGenerationParameters(new ECDomainParameters(ecParameters.Curve, ecParameters.G, ecParameters.N, ecParameters.H, ecParameters.GetSeed()), new SecureRandom()));
            return generator.GenerateKeyPair();
        }
        public static byte[] GetKeyShared(AsymmetricCipherKeyPair keyPair, byte[] remotePublicKey, bool isHash = false)
        {
            ECDHBasicAgreement keyAgreement = new();
            keyAgreement.Init(keyPair.Private);
            byte[] sharedKey = keyAgreement.CalculateAgreement(PublicKeyFactory.CreateKey(remotePublicKey)).ToByteArray();
            if (isHash)
            {
                return Hex.Decode(SHA3(Hex.ToHexString(sharedKey), 256));
            }
            else
            {
                if (sharedKey.Length != 32)
                {
                    sharedKey = Hex.Decode(Hex.ToHexString(sharedKey).PadLeft(64, '0')[^64..]);
                }
                return sharedKey;
            }

        }
    }
    public static byte[] GetPublicKey(this AsymmetricCipherKeyPair keyPair) => SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public).GetEncoded();
    public static string Encrypt(byte[] key, string plainText, SymmetricAlgorithmEnum algorithm = SymmetricAlgorithmEnum.Aes,
           CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7, byte[] iv = null, Encode encode = Encode.Hex)
    {
        byte[] cipherData = Encrypt(key, Encoding.UTF8.GetBytes(plainText), algorithm, cipherMode, paddingMode, iv);
        return encode == Encode.Hex ? Hex.ToHexString(cipherData) : Base64.ToBase64String(cipherData);
    }
    public static byte[] Encrypt(byte[] key, byte[] plainData, SymmetricAlgorithmEnum algorithm = SymmetricAlgorithmEnum.Aes,
            CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7, byte[] iv = null)
    {
        using SymmetricAlgorithm provider = SymmetricAlgorithm.Create(algorithm.ToString());
        provider.Key = key;
        provider.Mode = cipherMode;
        provider.Padding = paddingMode;
        if (iv == null)
        {
            provider.GenerateIV();
        }
        provider.IV = iv ?? new byte[provider.IV.Length];
        return provider.CreateEncryptor().TransformFinalBlock(plainData, 0, plainData.Length);
    }
    public enum SymmetricAlgorithmEnum
    {
        Aes, TripleDES
    }
    public enum Encode
    {
        Hex, Base64
    }
    public static string SHA3(string data, int size)
    {
        Sha3Digest hashAlgorithm = new(size);
        byte[] input = Encoding.ASCII.GetBytes(data);
        hashAlgorithm.BlockUpdate(input, 0, input.Length);
        byte[] result = new byte[size / 8]; // 512 / 8 = 64
        hashAlgorithm.DoFinal(result, 0);
        return Hex.ToHexString(result);
    }
}
