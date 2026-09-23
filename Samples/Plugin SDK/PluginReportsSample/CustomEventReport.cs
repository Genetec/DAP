// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomEvents;

public static class CustomEventReport
{
    public static Guid Id { get; } = new("975AB1E5-0F1C-43E7-B446-E4F005944E33"); // Replace with your own custom-event report ID.

    public static IReadOnlyList<CustomEvent> GetDefinitions(IEngine engine)
    {
        var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        return configuration.CustomEventService.CustomEvents.OrderBy(item => item.Name).ToList();
    }

    public const string Source = "SourceGuid";
    public const string Timestamp = "EventTimestamp";
    public const string Event = "EventId";
    public const string Message = "Message";
}

[DataContract]
public sealed class CustomEventFilterData
{
    [DataMember] public int? CustomEventId { get; set; }
    [DataMember] public string Message { get; set; }

    public string Serialize()
    {
        using var stream = new MemoryStream();
        new DataContractJsonSerializer(typeof(CustomEventFilterData)).WriteObject(stream, this);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static CustomEventFilterData Deserialize(string value)
    {
        if (string.IsNullOrEmpty(value)) return new CustomEventFilterData();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
        return (CustomEventFilterData)new DataContractJsonSerializer(typeof(CustomEventFilterData)).ReadObject(stream)
            ?? throw new SerializationException("The custom-event filter cannot be null.");
    }
}
