namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public class CertificateComparer
    {
        public static bool Equals(X509Certificate2 cert1, X509Certificate2 cert2)
        {
            if (ReferenceEquals(cert1, cert2))
            {
                return true;
            }

            if (cert1 == null || cert2 == null)
            {
                return false;
            }
      
            return string.Equals(cert1.Thumbprint, cert2.Thumbprint, StringComparison.OrdinalIgnoreCase);
        }

    }

}
