namespace Genetec.Dap.CodeSamples
{
    using System.Security.Cryptography;

    public class EncryptionKeyGenerator
    {
        public static byte[] GenerateEncryptionKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var key = new byte[32];
                rng.GetBytes(key);
                return key;
            }
        }
    }
}