namespace Genetec.Dap.CodeSamples
{
    using System;
    using Newtonsoft.Json;

    public class ExtraHiddenPayload
    {
        public string Text { get; set; }
        public int Numeric { get; set; }
        public bool Boolean { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }

        public string Serialize() => JsonConvert.SerializeObject(this);

        public static bool TryDeserialize(string json, out ExtraHiddenPayload payload)
        {
            try
            {
                payload = JsonConvert.DeserializeObject<ExtraHiddenPayload>(json);
                return true;
            }
            catch
            {
                payload = null;
                return false;
            }
        }
    }
}