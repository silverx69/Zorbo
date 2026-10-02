using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Zorbo.Net
{
    public class CertGenerationOptions
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public string PublicFile { get; set; }

        public string PrivateFile { get; set; }

        public IPAddress Address { get; set; }

        public List<string> HostNames { get; set; } = [];

        public List<IPAddress> LocalAddresses { get; set; } = [];
    }

    public static class Certificates
    {
        public static X509Certificate2 Generate(CertGenerationOptions options) {
            using RSA parent = RSA.Create(4096);
            using RSA rsa = RSA.Create(2048);

            CertificateRequest parentReq = new CertificateRequest(
                $"CN={options.Name}",
                parent,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            parentReq.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(true, false, 0, true));

            parentReq.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));

            parentReq.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], true));

            parentReq.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

            var san = new SubjectAlternativeNameBuilder();

            san.AddDnsName("localhost");

            foreach(string hostname in options.HostNames)
                if (!string.IsNullOrEmpty(hostname))
                    san.AddDnsName(hostname);

            if (options.Address is not null)
                san.AddIpAddress(options.Address);

            foreach (var address in options.LocalAddresses)
                if (address is not null)
                    san.AddIpAddress(address);

            parentReq.CertificateExtensions.Add(san.Build());

            using var parentCert = parentReq.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(365));

            File.WriteAllBytes(options.PrivateFile, parentCert.Export(X509ContentType.Pkcs12, options.Password));
            File.WriteAllBytes(options.PublicFile, parentCert.Export(X509ContentType.Cert));

            return X509CertificateLoader.LoadPkcs12FromFile(options.PrivateFile, options.Password);
        }

        public static bool SelfSignedValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            //the only error we need to correct with self-signed certs is 'UntrustedRoot'
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                return chain.ChainStatus.Any(s => s.Status == X509ChainStatusFlags.UntrustedRoot);

            return sslPolicyErrors == SslPolicyErrors.None; //?
        }
    }
}
