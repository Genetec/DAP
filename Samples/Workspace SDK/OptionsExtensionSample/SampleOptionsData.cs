// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows.Media;

    [DataContract(Namespace = "")]
    public class SampleOptionsData
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public Color Color { get; set; }

        public static SampleOptionsData Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data)) 
                return new SampleOptionsData();

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var serializer = new DataContractSerializer(typeof(SampleOptionsData));
            return (SampleOptionsData)serializer.ReadObject(stream);
        }

        public string Serialize()
        {
            using var stream = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(SampleOptionsData));
            serializer.WriteObject(stream, this);
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
