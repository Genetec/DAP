namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using Newtonsoft.Json;

    public class PluginConfiguration
    {
        [JsonIgnore]
        public X509Certificate2 Certificate { get; private set; }

        private string m_certificateBase64;

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static PluginConfiguration Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new PluginConfiguration();
            }

            try
            {
                return JsonConvert.DeserializeObject<PluginConfiguration>(data);
            }
            catch
            {
                return new PluginConfiguration();
            }
        }


        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (Certificate != null)
            {
                m_certificateBase64 = Convert.ToBase64String(Certificate.Export(X509ContentType.Pfx));
            }
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            m_certificateBase64 = null;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (!string.IsNullOrWhiteSpace(m_certificateBase64))
            {
                Certificate = new X509Certificate2(Convert.FromBase64String(m_certificateBase64));
                m_certificateBase64 = null;       
            }
        }
    }
}