using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Softwarekueche.Web;

public class SignatureUtil
{
    public static bool Verify(string data, string signature, X509Certificate2 serverCert)
    {
        try
        {
            using var publicKey = serverCert.GetRSAPublicKey();
            var dataByteArray = Encoding.UTF8.GetBytes(data);
            var signatureByteArray = Convert.FromBase64String(signature);

            return publicKey.VerifyData(
                data: dataByteArray,
                signature: signatureByteArray,
                hashAlgorithm: HashAlgorithmName.SHA256,
                padding: RSASignaturePadding.Pkcs1);
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}