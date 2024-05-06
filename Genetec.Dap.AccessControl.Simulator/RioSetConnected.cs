namespace Genetec.Dap
{
    using System.Xml.Linq;

    public class RioSetConnected
    {
        public string InterfaceName { get; set; }
        public bool SetConnected { get; set; }

        public RioSetConnected(string interfaceName, bool setConnected)
        {
            InterfaceName = interfaceName;
            SetConnected = setConnected;
        }

        public override string ToString()
        {
            return new XElement("Request",
                new XElement("BusUpdate",
                    new XElement("SetConnected",
                        new XElement("Interface", InterfaceName),
                        new XElement("IsConnected", SetConnected),
                        new XElement("SpecificDevices",
                            new XElement("None"))))).ToString();
        }
    }
}
