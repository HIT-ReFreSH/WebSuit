using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        try
        {
            // 加载私钥
            string privateKey = File.ReadAllText("private_key.pem");
            using var rsaPrivate = RSA.Create();
            rsaPrivate.ImportFromPem(privateKey.ToCharArray());

            // 要签名的数据
            string dataToSign = "Hello, RSA!";
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

            // 使用私钥对数据进行签名
            byte[] signature = rsaPrivate.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Console.WriteLine("Signature: " + Convert.ToBase64String(signature));

            // 加载公钥
            string publicKey = File.ReadAllText("public_key.pem");
            using var rsaPublic = RSA.Create();
            rsaPublic.ImportFromPem(publicKey.ToCharArray());

            // 使用公钥验证签名
            bool isVerified = rsaPublic.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Console.WriteLine("Signature Verified: " + isVerified);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Cryptographic error: " + e.Message);
        }
    }
}