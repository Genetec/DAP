namespace Genetec.Dap
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    public class RioChannel
    {
        /// <summary>
        /// Gets or sets the <see cref="RioChannel"/> name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RioChannel"/> interfaces
        /// </summary>
        public List<RioInterface> Interfaces { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RioChannel"/> allowed users
        /// </summary>
        public List<string> AllowUsers { get; set; }

        /// <summary>
        /// Return serialised(xml) string of rio channel 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _serialize().ToString();
        }

        /// <summary>
        /// Serialize this class into xml 
        /// </summary>
        /// <returns></returns>
        private XElement _serialize()
        {
            var interfaces = new List<XElement>();

            foreach (var rioInterface in Interfaces)
            {
                // Input labels node
                var inputLabelsXml = new XElement("InputLabels");

                foreach (var input in rioInterface.Inputs)
                {
                    inputLabelsXml.Add(new XElement("String", input));
                }

                // Output labels node
                var outputLabelsXml = new XElement("OutputLabels");

                foreach (var output in rioInterface.Outputs)
                {
                    outputLabelsXml.Add(new XElement("String", output));
                }

                // Reader labels node
                var readerLabelsXml = new XElement("ReaderLabels");

                foreach (var reader in rioInterface.Readers)
                {
                    readerLabelsXml.Add(new XElement("String", reader));
                }

                var interfaceXml = new XElement("Interface",
                    new XElement("CustomInterface",
                        new XElement("Address", rioInterface.Address),
                        new XElement("Template",
                            new XElement("Manufacturer", rioInterface.Manufacturer),
                            new XElement("ModelName", rioInterface.ModelName),
                            new XElement("Description", rioInterface.Description),
                            inputLabelsXml, // XElement with all input childs 
                            outputLabelsXml, // XElement with all output childs 
                            readerLabelsXml // XElement with all reader childs
                        )
                    )
                );

                interfaces.Add(interfaceXml);
            }

            var allowUsersXml = new XElement("AllowUsers");

            foreach (var user in AllowUsers)
            {
                allowUsersXml.Add(new XElement("String", user));
            }

            var interfaceXmls = interfaces.ToArray();

            var rootXml = new XElement("Request",
                new XElement("RioBus",
                    new XElement("Interfaces", interfaceXmls),
                    allowUsersXml
                )
            );

            return rootXml;
        }

        public RioChannel()
        {
            AllowUsers = new List<string>();
            Interfaces = new List<RioInterface>();
        }
    }
}
