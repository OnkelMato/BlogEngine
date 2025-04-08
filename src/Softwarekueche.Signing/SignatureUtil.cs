using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Softwarekueche.Signing;

public class SignatureUtil
{
    public static string Sign(string data, X509Certificate2 clientCert)
    {
        using var privateKey = clientCert.GetRSAPrivateKey();
        var dataByteArray = Encoding.UTF8.GetBytes(data);
        var signatureByteArray = privateKey.SignData(
            data: dataByteArray,
            hashAlgorithm: HashAlgorithmName.SHA256,
            padding: RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signatureByteArray);
    }
}