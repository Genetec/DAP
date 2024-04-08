namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public class CertificateGenerator
    {
        public static X509Certificate2 CreateSelfSigned(string subjectName, int expirationDays = 365)
        {
            using (RSA rsa = RSA.Create(2048))
            {
              
                var request = new CertificateRequest("CN=" + subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                DateTimeOffset start = DateTime.UtcNow.AddDays(-1);
                DateTimeOffset end = start.AddDays(expirationDays);

                return request.CreateSelfSigned(start, end);
            }
        }
    }
}
