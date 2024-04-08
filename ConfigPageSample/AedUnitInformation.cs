// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;

    public class AedUnitInformation
    {
        public DateTime LastInspectionDate { get; set; }

        public DateTime NextScheduledMaintenance { get; set; }

        public DateTime BatteryExpirationDate { get; set; }

        public DateTime PadExpirationDate { get; set; }

        public static AedUnitInformation Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new AedUnitInformation();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var serializer = new DataContractSerializer(typeof(AedUnitInformation));
                return (AedUnitInformation)serializer.ReadObject(stream);
            }
        }

        public string Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(AedUnitInformation));
                serializer.WriteObject(stream, this);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}