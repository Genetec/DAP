// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Namespace = "")]
    public class AedUnitInformation
    {
        private static readonly DataContractSerializer s_serializer = new(typeof(AedUnitInformation));

        [DataMember] 
        public DateTime LastInspectionDate { get; set; }

        [DataMember] 
        public DateTime NextScheduledMaintenance { get; set; }

        [DataMember] 
        public DateTime BatteryExpirationDate { get; set; }

        [DataMember] 
        public DateTime PadExpirationDate { get; set; }

        // Deserialize the data from an XML string
        public static AedUnitInformation Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new AedUnitInformation();
            }

            using var stringReader = new StringReader(data);
            using var xmlReader = XmlReader.Create(stringReader);
            return (AedUnitInformation)s_serializer.ReadObject(xmlReader);
        }

        // Serialize the data to an XML string
        public string Serialize()
        {
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter);
            s_serializer.WriteObject(xmlWriter, this);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }
    }
}
