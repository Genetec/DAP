namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public static class X509Certificate2Extensions
    {
        public static byte[] EncryptUsingPublicKey(this X509Certificate2 certificate, byte[] data)
        {
            using (RSA publicKey = certificate.GetRSAPublicKey())
            {
                if (publicKey == null)
                    throw new InvalidOperationException("Failed to retrieve RSA public key from the certificate.");

                return publicKey.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            }
        }


        public static byte[] DecryptUsingPrivateKey(this X509Certificate2 certificate, byte[] data)
        {
            using (RSA privateKey = certificate.GetRSAPrivateKey())
            {
                if (privateKey == null)
                    throw new InvalidOperationException("Failed to retrieve RSA public key from the certificate.");

                return privateKey.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
            }
        }
    }
}