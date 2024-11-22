// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Buffers.Binary;
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
        foreach (var fileName in _authorizedKeys)
        {
            if (!File.Exists(fileName))
            {
                logger.LogWarning("Missing AuthorizedKeys file: {filename}", fileName);
                continue;
            }


            yield return File.ReadAllText(fileName);
        }
    }

    /// <summary>
    /// Verify message with public keys
    /// </summary>
    /// <param name="encryptedMessage">Origin message</param>
    /// <param name="signature">Base64 encoded signed bytes</param>
    /// <returns></returns>
    public bool VerifySignature(string encryptedMessage, string signature)
    {
        var authorizedKeys = GetAuthorizedKeys();

        return authorizedKeys.Any(publicKey => VerifyWithPublicKey(publicKey, encryptedMessage, signature));
    }


    private bool VerifyWithPublicKey(string publicKey, string data, string signature)
    {
        try
        {
            using var rsaPublic = RSA.Create();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);
            rsaPublic.ImportFromPem(publicKey.ToCharArray());

            return rsaPublic.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Encrypt the given data with given key file
    /// </summary>
    /// <param name="privateKeyFile">Path to private key file</param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] Encrypt(string privateKeyFile, string data)
    {
        string privateKey = File.ReadAllText(privateKeyFile);
        using var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKey.ToCharArray());
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        // 使用私钥对数据进行签名
        return rsaPrivate.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}