namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    public class SecureKeyConverter
    {
        public static SecureString ConvertToSecureString(byte[] data)
        {
            var secureString = new SecureString();

            try
            {
                foreach (byte b in data)
                {
                    secureString.AppendChar((char)b);
                }

                secureString.MakeReadOnly();
                return secureString;
            }
            catch (Exception)
            {
                secureString.Dispose();
                throw;
            }
        }

        public static byte[] ConvertToByteArray(SecureString secureString)
        {
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                if (ptr == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to convert SecureString to byte array.");


                byte[] bytes = new byte[secureString.Length * 2];
                Marshal.Copy(ptr, bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}