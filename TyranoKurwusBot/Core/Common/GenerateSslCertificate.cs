using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TyranoKurwusBot.Core.Common;

public static class GenerateSslCertificate
{
    private static X509Certificate2? _generated;
    public static X509Certificate2 Certificate => _generated ?? throw new FieldAccessException("You have to generate this certificate first");

    public static void Generate(string commonName)
    {
        _generated = GetSelfSignedCertificate(commonName);
    }

    private static X509Certificate2 GetSelfSignedCertificate(string commonName)
    {
        var password = Guid.NewGuid().ToString();
        const int rsaKeySize = 2048;
        const int years = 5;
        var hashAlgorithm = HashAlgorithmName.SHA256;

        using var rsa = RSA.Create(rsaKeySize);
        var request = new CertificateRequest($"cn={commonName}", rsa, hashAlgorithm, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                X509KeyUsageFlags.DigitalSignature, false)
        );
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false)
        );

        var certificate =
            request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(years));

        // Return the PFX exported version that contains the key
        return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet);
    }
}