// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

// A single name/value pair, used for both query parameters and headers.
[DataContract]
public class NameValuePair
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Value { get; set; }
}

/// <summary>
/// This class represents an action that sends an HTTP request to a configured endpoint.
/// </summary>
[DataContract]
public class SendHttpRequestAction
{
    private static readonly DataContractJsonSerializer s_serializer = new(typeof(SendHttpRequestAction));

    // The HTTP method of the request, for example GET or POST.
    [DataMember]
    public string Method { get; set; } = "GET";

    // The absolute HTTP or HTTPS endpoint URL.
    [DataMember]
    public string Url { get; set; } = "https://";

    // The query-string parameters of the request.
    [DataMember]
    public List<NameValuePair> QueryParameters { get; set; } = new();

    // The custom HTTP headers of the request.
    [DataMember]
    public List<NameValuePair> Headers { get; set; } = new();

    // The content type of the request body, for example "application/json".
    [DataMember]
    public string ContentType { get; set; } = "application/json";

    // The raw body sent with POST, PUT, and PATCH requests.
    [DataMember]
    public string Body { get; set; }

    // Serialize the object to a JSON string.
    public string Serialize()
    {
        using var stream = new MemoryStream();
        s_serializer.WriteObject(stream, this);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Deserialize the JSON string to an object.
    public static SendHttpRequestAction Deserialize(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            return (SendHttpRequestAction)s_serializer.ReadObject(stream);
        }
        catch (SerializationException)
        {
            // Log the exception if needed
            return null;
        }
    }
}
