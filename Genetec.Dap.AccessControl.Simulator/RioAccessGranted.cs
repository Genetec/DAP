namespace Genetec.Dap
{
    using System.Xml.Linq;

    public class RioAccessGranted
    {
        private XElement root;

        public string Interface { get; set; }
        public string Reader { get; set; }
        public string Timestamp { get; set; }
        public string BitCount { get; set; }
        public string CardCode { get; set; }


        public RioAccessGranted(string interfaceName, string reader, string timestamp, string bitCount, string cardCode, bool granted)
        {
            Interface = interfaceName;
            Reader = reader;
            Timestamp = timestamp;
            BitCount = bitCount;
            CardCode = cardCode;

            var rootXml = new XElement("Request",
                new XElement("BusUpdate",
                    new XElement("OfflineDecision",
                        new XElement("Interface", interfaceName),
                        new XElement("Reader", reader),
                        new XElement("Timestamp", timestamp),
                        new XElement("Card", 
                            new XElement("Some", 
                                new XElement("BitCount", bitCount),
                                new XElement("CardCode", cardCode)
                            )
                        ),
                        new XElement("Granted",granted.ToString())
                    )
                )
            );

            root = rootXml;
        }

        public override string ToString()
        {
            return root.ToString();
        }
    }
}
