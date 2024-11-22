// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Security.Cryptography;
using System.Text;
using HitRefresh.MobileSuit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;

namespace HitRefresh.WebSuit.Services;

public class KeyChainService(IConfiguration configuration, ILogger<KeyChainService> logger)
{
    private readonly string[] _authorizedKeys = configuration.GetSection("AuthorizedKeys").Get<string[]>() ?? [];

    private IEnumerable<string> GetAuthorizedKeys()
    {

        foreach (string key in _authorizedKeys)
        {
            if (key.StartsWith("file::"))
            {
                var fileName = key.Substring("file::".Length);
                if (!File.Exists(fileName))
                {
                    logger.LogWarning("Missing AuthorizedKeys file: {filename}",fileName);
                    continue;
                }

                foreach (var fileKey in File.ReadAllLines(fileName))
                {
                    if (IsValidKeyFormat(fileKey))
                    {
                        yield return fileKey.Trim();
                    }
                    logger.LogWarning("Invalid Authorized Key: {key}",fileKey);
                }
            }
            else
            {
                if (IsValidKeyFormat(key))
                {
                    yield return key.Trim();
                }
                logger.LogWarning("Invalid Authorized Key: {key}",key);
            }
        }
    }

    public bool VerifySignature(string encryptedMessage, string signature)
    {
        var authorizedKeys = GetAuthorizedKeys();

        return authorizedKeys.Any(publicKey => VerifyWithPublicKey(publicKey, encryptedMessage, signature));
    }

    private bool IsValidKeyFormat(string key)
    {
        // 验证公钥格式支持扩展
        return key.StartsWith("ssh-rsa ") ||
               key.StartsWith("ecdsa-sha2-nistp") ||
               key.StartsWith("ssh-ed25519 ") ||
               key.StartsWith("-----BEGIN PUBLIC KEY-----");
    }

    private bool VerifyWithPublicKey(string publicKey, string data, string signature)
    {
        try
        {
            if (publicKey.StartsWith("ssh-rsa"))
            {
                return VerifyRsaPublicKey(publicKey, data, signature);
            }
            else if (publicKey.StartsWith("ecdsa-sha2-nistp"))
            {
                return VerifyEcdsaPublicKey(publicKey, data, signature);
            }
            else if (publicKey.StartsWith("ssh-ed25519"))
            {
                return VerifyEd25519PublicKey(publicKey, data, signature);
            }
            else if (publicKey.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return VerifyPemPublicKey(publicKey, data, signature);
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyRsaPublicKey(string publicKey, string data, string signature)
    {
        string base64Key = publicKey.Split(' ')[1];
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(base64Key), out _);
        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(data),
            Convert.FromBase64String(signature),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    private bool VerifyEcdsaPublicKey(string publicKey, string data, string signature)
    {
        string base64Key = publicKey.Split(' ')[1];
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(base64Key), out _);
        return ecdsa.VerifyData(
            Encoding.UTF8.GetBytes(data),
            Convert.FromBase64String(signature),
            HashAlgorithmName.SHA256);
    }

    private bool VerifyEd25519PublicKey(string publicKey, string data, string signature)
    {
        try
        {
            // 提取 Ed25519 公钥
            string base64Key = publicKey.Split(' ')[1];
            byte[] keyBytes = Convert.FromBase64String(base64Key);

            // 创建 Ed25519 公钥对象
            var algorithm = SignatureAlgorithm.Ed25519;
            var publicKeyObj = PublicKey.Import(algorithm, keyBytes, KeyBlobFormat.RawPublicKey);

            // 验证签名
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return algorithm.Verify(publicKeyObj, messageBytes, signatureBytes);
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyPemPublicKey(string publicKey, string data, string signature)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(data),
            Convert.FromBase64String(signature),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }
}