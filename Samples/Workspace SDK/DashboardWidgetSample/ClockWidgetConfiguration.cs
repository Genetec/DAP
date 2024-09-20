// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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