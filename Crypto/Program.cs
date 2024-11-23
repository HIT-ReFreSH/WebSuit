using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static void Main()
    {
        try
        {
            // 加载私钥
            var privateKey = File.ReadAllText("private_key.pem");
            using var rsaPrivate = RSA.Create();
            rsaPrivate.ImportFromPem(privateKey.ToCharArray());

            // 要签名的数据
            var dataToSign = "Hello, RSA!";
            var dataBytes = Encoding.UTF8.GetBytes(dataToSign);

            // 使用私钥对数据进行签名
            var signature = rsaPrivate.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Console.WriteLine("Signature: " + Convert.ToBase64String(signature));

            // 加载公钥
            var publicKey = File.ReadAllText("public_key.pem");
            using var rsaPublic = RSA.Create();
            rsaPublic.ImportFromPem(publicKey.ToCharArray());

            // 使用公钥验证签名
            var isVerified = rsaPublic.VerifyData
                (dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Console.WriteLine("Signature Verified: " + isVerified);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Cryptographic error: " + e.Message);
        }
    }
}