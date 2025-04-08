using DotMake.CommandLine;
using System.Security.Cryptography.X509Certificates;

namespace Softwarekueche.Signing
{
    internal class Program
    {


        static void Main(string[] args)
        {
            Cli.Run<SignFileCommand>(args);
        }
    }

    [CliCommand(Description = "Sign file command")]
    public class SignFileCommand
    {
        [CliOption(Description = "Filename for private key")]
        public string PrivateKey { get; set; } = "certificate.key";

        [CliOption(Description = "Filename for public key")]
        public string PublicKey { get; set; } = "certificate.crt";

        [CliOption(Description = "Filename of the string to sign")]
        public string Source { get; set; } = null!;

        [CliOption(Description = "Filename of the output file", Required = false)]
        public string? Signature { get; set; }

        public void Run(CliContext context)
        {
            if (!File.Exists(Source))
                throw new FileNotFoundException($"File {Source} not found.");
            if (!File.Exists(PrivateKey))
                throw new FileNotFoundException($"File {PrivateKey} not found.");
            if (!File.Exists(PublicKey))
                throw new FileNotFoundException($"File {PublicKey} not found.");

            Signature ??= $"{Source}.sig";

            var json = File.ReadAllText(Source);
            var cert = X509Certificate2.CreateFromPemFile(PublicKey, PrivateKey);
            var signature = SignatureUtil.Sign(json, cert);

            File.WriteAllText(Signature, signature);
        }
    }
}
