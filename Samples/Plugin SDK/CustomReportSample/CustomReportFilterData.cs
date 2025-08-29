// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

/// <summary>
/// This class represents the filter data for this custom report sample.
/// It is used to pass data from the client to the server when running a custom report.
/// </summary>
public class CustomReportFilterData
{
    private static readonly DataContractJsonSerializer s_serializer = new(typeof(CustomReportFilterData));

    public string Message { get; set; }
  
    public TimeSpan Duration { get; set; }
 
    public int NumericValue { get; set; }

    public decimal DecimalValue { get; set; }
    
    public bool Enabled { get; set; }

    public int? CustomEvent { get; set; }

    // Deserialize the filter data from a JSON string
    public static CustomReportFilterData Deserialize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new CustomReportFilterData();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
        return (CustomReportFilterData)s_serializer.ReadObject(stream);
    }

    // Serialize the filter data to a JSON string
    public string Serialize()
    {
        var serializer = new DataContractJsonSerializer(typeof(CustomReportFilterData));
        using var stream = new MemoryStream();
        serializer.WriteObject(stream, this);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}