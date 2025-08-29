// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Genetec.Dap.CodeSamples;

/// <summary>
/// Represents the configuration of Role (Plugin)
/// This class is serialized as a JSON string and stored in the <c>SpecificConfiguration</c> property of the Role class.
/// </summary>
public class RoleConfiguration : INotifyPropertyChanged
{
    // Default values for IP address and port
    private IPAddress m_ipAddress = IPAddress.Loopback;
    private int m_port = 5660;

    public event PropertyChangedEventHandler PropertyChanged;

    [JsonIgnore]
    public IPAddress IPAddress
    {
        get => m_ipAddress;
        set => SetProperty(ref m_ipAddress, value);
    }

    public int Port
    {
        get => m_port;
        set => SetProperty(ref m_port, value);
    }

    /// <summary>
    /// Private property for JSON serialization of IP address.
    /// Converts between string and IPAddress.
    /// </summary>
    [JsonProperty(nameof(IPAddress))]
    private string IPAddressString
    {
        get => m_ipAddress.ToString();
        set
        {
            if (IPAddress.TryParse(value, out var ipAddress))
            {
                IPAddress = ipAddress;
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Deserializes a JSON string into a RoleConfiguration object.
    /// </summary>
    /// <param name="data">JSON string to deserialize</param>
    /// <returns>A new RoleConfiguration object, or default if data is null or empty</returns>
    public static RoleConfiguration Deserialize(string data)
    {
        return string.IsNullOrEmpty(data)
            ? new RoleConfiguration()
            : JsonConvert.DeserializeObject<RoleConfiguration>(data);
    }

    /// <summary>
    /// Populates this object with data from a JSON string.
    /// </summary>
    /// <param name="data">JSON string containing object data</param>
    public void Load(string data) => JsonConvert.PopulateObject(data, this);

    /// <summary>
    /// Serializes this object to a JSON string.
    /// </summary>
    /// <returns>A JSON string representation of this object</returns>
    public string Serialize() => JsonConvert.SerializeObject(this);
}