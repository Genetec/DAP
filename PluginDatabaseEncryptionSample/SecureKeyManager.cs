namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using Genetec.Sdk.Entities;

    public class SecureKeyManager
    {
        private readonly IRestrictedConfiguration m_configuration;

        private readonly X509Certificate2 m_certificate;

        public SecureKeyManager(IRestrictedConfiguration configuration)
        {
            m_configuration = configuration;

            if (configuration.TryGetPrivateValue("Certificate", out var privateValue))
            {
                m_certificate = CertificateSerializer.Deserialize(privateValue);
            }

            m_certificate = CertificateGenerator.CreateSelfSigned("Genetec Suprema Plugin");
            configuration.SetPrivateValue("Certificate", CertificateSerializer.Serialize(m_certificate));
        }


        public byte[] Encrypt(byte[] data) => m_certificate.EncryptUsingPublicKey(data);

        public byte[] Decrypt(byte[] data) => m_certificate.DecryptUsingPrivateKey(data);

    }

    public static class CertificateSerializer
    {

        public static SecureString Serialize(X509Certificate2 certificate)
        {

            byte[] rawData = certificate.Export(X509ContentType.Pfx);
            string base64Data = Convert.ToBase64String(rawData);

            var secureString = new SecureString();
            foreach (char c in base64Data)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();

            return secureString;
        }


        public static X509Certificate2 Deserialize(SecureString secureString)
        {

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                string base64Data = Marshal.PtrToStringUni(ptr);

                byte[] rawData = Convert.FromBase64String(base64Data);

                return new X509Certificate2(rawData);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}