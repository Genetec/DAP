namespace Genetec.Dap
{
    using System.Xml.Linq;

    public class RioCardDetected
    {

        public string Interface { get; set; }
        public string Reader { get; set; }
        public int BitCount { get; set; }
        public string CardCode { get; set; }


        public RioCardDetected(string @interface, string reader, int bitCount, string cardCode)
        {
            Interface = @interface;
            Reader = reader;
            BitCount = bitCount;
            CardCode = cardCode;
        }

        public override string ToString()
        {
          return new XElement("Request",
               new XElement("BusUpdate",
                   new XElement("CardSwipe",
                       new XElement("Interface", Interface),
            new XElement("Reader", Reader),
                       new XElement("BitCount", BitCount),
                       new XElement("CardCode", CardCode)))).ToString();
        }

    }
}
