// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using Sdk;
using Sdk.Entities;
using Sdk.Workspace.Pages;

/// <summary>
///     Represents a custom configuration page for a specific role type.
///     This class extends ConfigPage to provide custom configuration options for a role.
/// </summary>
class CustomConfigPage : ConfigPage
{
    // Configuration object to handle serialization and deserialization
    private readonly RoleConfiguration m_configuration = new();

    // Configuration properties
    private IPAddress m_ipAddress;
    private int m_port;
    private SecureString m_password;
    private SecureString m_userName;

    private Role m_role; // The role entity being configured

    protected override EntityType EntityType => EntityType.Role; // Only show this page for roles

    protected override Guid Entity
    {
        set
        {
            // Check if the entity is a Role of the correct subtype
            if (Workspace.Sdk.GetEntity(value) is Role entity && entity.SubType == PluginTypes.SamplePlugin)
            {
                m_role = entity; // Store the role entity
                IsVisible = true; // Show the configuration page
            }
            else
            {
                IsVisible = false; // Hide the configuration page
            }
        }
    }

    public IPAddress IPAddress
    {
        get => m_ipAddress;
        set
        {
            if (SetProperty(ref m_ipAddress, value))
            {
                IsDirty = true;
            }
        }
    }

    public int Port
    {
        get => m_port;
        set
        {
            if (SetProperty(ref m_port, value))
            {
                IsDirty = true;
            }
        }
    }

    public SecureString Password
    {
        get => m_password;
        set
        {
            if (SetProperty(ref m_password, value))
            {
                IsDirty = true;
            }
        }
    }

    public SecureString UserName
    {
        get => m_userName;
        set
        {
            if (SetProperty(ref m_userName, value))
            {
                IsDirty = true;
            }
        }
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    ///     Initializes the configuration page by setting up the view.
    ///     This method is called when the page is first created.
    /// </summary>
    protected override void Initialize()
    {
        View = new CustomConfigPageView { DataContext = this };
    }

    /// <summary>
    ///     Refreshes the configuration page with the current role configuration.
    ///     This method resets the IsDirty flag to false as it loads the saved configuration.
    /// </summary>
    protected override void Refresh()
    {
        m_configuration.Load(m_role.SpecificConfiguration);
        IPAddress = m_configuration.IPAddress;
        Port = m_configuration.Port;
        IsDirty = false;
    }

    /// <summary>
    ///     Saves the current configuration to the role.
    ///     This method is called when changes need to be persisted, typically when IsDirty is true.
    ///     It updates both the specific configuration and the restricted configuration (for sensitive data).
    /// </summary>
    protected override void Save()
    {
        m_configuration.IPAddress = IPAddress;
        m_configuration.Port = Port;

        Workspace.Sdk.TransactionManager.ExecuteTransaction(UpdateConfiguration);

        void UpdateConfiguration()
        {
            // Update the role's specific configuration
            m_role.SpecificConfiguration = m_configuration.Serialize();

            IRestrictedConfiguration restrictedConfiguration = m_role.RestrictedConfiguration;

            // Update username if changed
            if (m_userName is not null)
            {
                restrictedConfiguration.SetPrivateValue("Username", m_userName);
                m_userName = null;

                if (m_role.IsOnline)
                {
                    // Notify the server that the username has been updated 
                    _ = Workspace.Sdk.RequestManager.SendRequestAsync<RestrictedConfigurationValueChanged, VoidResponse>(m_role.Guid, new RestrictedConfigurationValueChanged("Username"));
                }
            }

            // Update password if changed
            if (m_password is not null)
            {
                restrictedConfiguration.SetPrivateValue("Password", m_password);
                m_password = null;

                if (m_role.IsOnline)
                {
                    // Notify the server that the password has been updated
                    _ = Workspace.Sdk.RequestManager.SendRequestAsync<RestrictedConfigurationValueChanged, VoidResponse>(m_role.Guid, new RestrictedConfigurationValueChanged("Password"));
                }
            }
        }
    }
}