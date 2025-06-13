// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    class ClockWidgetConfiguration
    {
        [DataMember]
        public bool ShowDigitalTime { get; set; }

        public static ClockWidgetConfiguration Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new ClockWidgetConfiguration();

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var serializer = new DataContractJsonSerializer(typeof(ClockWidgetConfiguration));

            return (ClockWidgetConfiguration)serializer.ReadObject(stream);
        }

        public string Serialize()
        {
            using var stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(ClockWidgetConfiguration));
            serializer.WriteObject(stream, this);
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}