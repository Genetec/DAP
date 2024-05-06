namespace Genetec.Dap
{
    using System.Collections.Generic;

    public class RioInterface
    {
        public string Address { get; set; }

 
        public string Manufacturer { get; set; }
        public string ModelName { get; set; }
        public string Description { get; set; }
        public List<string> Inputs { get; } = new List<string>();
        public List<string> Outputs { get; } = new List<string>();
        public List<string> Readers { get; } = new List<string>();

    }
}