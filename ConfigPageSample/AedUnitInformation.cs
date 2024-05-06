// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Namespace = "")]
    public class AedUnitInformation
    {
        [DataMember]
        public DateTime LastInspectionDate { get; set; }

        [DataMember]
        public DateTime NextScheduledMaintenance { get; set; }

        [DataMember]
        public DateTime BatteryExpirationDate { get; set; }

        [DataMember]
        public DateTime PadExpirationDate { get; set; }

        private static readonly DataContractSerializer s_serializer = new DataContractSerializer(typeof(AedUnitInformation));

        public static AedUnitInformation Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new AedUnitInformation();

            using var stringReader = new StringReader(data);
            using var xmlReader = XmlReader.Create(stringReader);
            return (AedUnitInformation)s_serializer.ReadObject(xmlReader);
        }

        public string Serialize()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    s_serializer.WriteObject(xmlWriter, this);
                    xmlWriter.Flush();
                    return stringWriter.ToString();
                }
            }
        }
    }

}