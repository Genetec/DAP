// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

/// <summary>
/// The role's configuration, serialized as JSON in the SpecificConfiguration property.
/// </summary>
[DataContract]
public class RoleConfiguration
{
    private static readonly DataContractJsonSerializer s_serializer = new(typeof(RoleConfiguration));

    // The TCP port the plugin listens on for local HTTP requests.
    [DataMember]
    public int Port { get; set; } = 8085;

    // Deserializes a JSON string into a RoleConfiguration object, or returns the defaults
    // when the string is empty.
    public static RoleConfiguration Deserialize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new RoleConfiguration();
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
        return (RoleConfiguration)s_serializer.ReadObject(stream);
    }

    // Serializes this object to a JSON string.
    public string Serialize()
    {
        using var stream = new MemoryStream();
        s_serializer.WriteObject(stream, this);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
