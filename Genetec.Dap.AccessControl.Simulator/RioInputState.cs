namespace Genetec.Dap
{
    using System.Xml.Linq;

    public class RioInputState
    {
     

        public string State { get; set; }
        public string Input { get; set; }
        public string Interface { get; set; }

        public RioInputState(string interfaceName, string input, string state)
        {
            State = state;
            Input = input;

            Interface = interfaceName;

      
        }

        public override string ToString()
        {
            return new XElement("Request",
               new XElement("BusUpdate",
                   new XElement("InputStatusUpdate",
                       new XElement("Interface", Interface),
                       new XElement("Input", Input),
                       new XElement("State", new XElement(State))))).ToString();
        }
    }
}
