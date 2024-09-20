// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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