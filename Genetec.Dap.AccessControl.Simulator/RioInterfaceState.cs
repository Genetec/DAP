namespace Genetec.Dap
{
    using System.Xml.Linq;

    public class RioInterfaceState
    {
        

        public bool Connected { get; set; }
        public string Interface { get; set; }

        public RioInterfaceState(string interfaceName, bool connected)
        {
            Interface = interfaceName;
            Connected = connected;
        }


        public override string ToString()
        {
            return new XElement("Request",
                new XElement("BusUpdate",
                    new XElement("SetConnected",
                        new XElement("Interface", Interface),
                        new XElement("IsConnected", Connected),
                        new XElement("SpecificDevices", new XElement("None"))
                    )
                )
            ).ToString();
        }
    }
}
