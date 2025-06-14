// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System.Runtime.Serialization;

/// <summary>
/// Represents a change in a restricted configuration value.
/// This class is designed to be serializable using DataContractSerializer.
/// </summary>
[DataContract]
public class RestrictedConfigurationValueChanged
{
    /// <summary>
    /// Gets the key of the configuration value that changed.
    /// This property is serialized as part of the DataContract.
    /// </summary>
    [DataMember]
    public string Key { get; private set; }

    /// <summary>
    /// Initializes a new instance of the RestrictedConfigurationValueChanged class with the specified key.
    /// </summary>
    /// <param name="key">The key of the configuration value that changed.</param>
    public RestrictedConfigurationValueChanged(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Parameterless constructor for serialization.
    /// This constructor is required for proper deserialization by DataContractSerializer.
    /// It's private to prevent direct instantiation without a key.
    /// </summary>
    private RestrictedConfigurationValueChanged()
    {
    }
}