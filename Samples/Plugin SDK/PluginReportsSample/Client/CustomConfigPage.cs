// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Runtime.CompilerServices;
using Sdk;
using Sdk.Entities;
using Sdk.Workspace.Pages;

/// <summary>
/// The configuration page of the ingestion role.
/// </summary>
internal class CustomConfigPage : ConfigPage
{
    private RoleConfiguration m_configuration = new();

    private int m_port;

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

    protected override void Initialize()
    {
        View = new CustomConfigPageView { DataContext = this };
    }

    protected override void Refresh()
    {
        m_configuration = RoleConfiguration.Deserialize(m_role.SpecificConfiguration);
        Port = m_configuration.Port;
        IsDirty = false;
    }

    protected override void Save()
    {
        m_configuration.Port = Port;
        m_role.SpecificConfiguration = m_configuration.Serialize();
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
}
