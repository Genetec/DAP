// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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