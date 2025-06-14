// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// This class represents an action to launch an encoder command.
/// </summary>
[DataContract]
public class LaunchEncoderCommandAction
{
    private static readonly DataContractJsonSerializer s_serializer = new(typeof(LaunchEncoderCommandAction));

    // The camera to which the command will be sent.
    [DataMember]
    public Guid Camera { get; set; }

    // The command to send to the encoder.
    [DataMember]
    public uint EncoderCommand { get; set; }

    // Serialize the object to a JSON string.
    public string Serialize()
    {
        using var stream = new MemoryStream();
        s_serializer.WriteObject(stream, this);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Deserialize the JSON string to an object.
    public static LaunchEncoderCommandAction Deserialize(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            return (LaunchEncoderCommandAction)s_serializer.ReadObject(stream);
        }
        catch (SerializationException)
        {
            // Log the exception if needed
            return null;
        }
    }
}