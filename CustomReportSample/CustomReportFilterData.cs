// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    public class CustomReportFilterData
    {
        private static readonly DataContractJsonSerializer s_serializer =
            new DataContractJsonSerializer(typeof(CustomReportFilterData));

        [DataMember] 
        public string Message { get; set; }

        public static CustomReportFilterData Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new CustomReportFilterData();

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            return (CustomReportFilterData)s_serializer.ReadObject(stream);
        }

        public string Serialize()
        {
            var serializer = new DataContractJsonSerializer(typeof(CustomReportFilterData));
            using var stream = new MemoryStream();
            serializer.WriteObject(stream, this);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}