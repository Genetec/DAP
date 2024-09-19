// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server;

using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Client;
using Sdk.Workflows;
using System.Security;
using Genetec.Dap.CodeSamples;
using System.Runtime.InteropServices;

[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin
{
    private IDisposable m_disposable;

    private readonly RoleConfiguration m_configuration = new();

    private Role m_role;

    protected override void OnPluginLoaded()
    {
        m_configuration.PropertyChanged += OnConfigurationPropertyChanged;
        Engine.RequestManager.AddAsyncRequestHandler<RestrictedConfigurationValueChanged, VoidResponse>(OnRestrictedConfigurationValueChanged);

        m_role = (Role)Engine.GetEntity(PluginGuid);
        m_disposable = m_role.ObserveSpecificConfiguration().Subscribe(m_configuration.Load);
    }

    private void OnConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(RoleConfiguration.IPAddress):
                // Handle IP address change
                break;
            case nameof(RoleConfiguration.Port):
                // Handle port change
                break;
        }
    }

    private Task<VoidResponse> OnRestrictedConfigurationValueChanged(RestrictedConfigurationValueChanged request, RequestManagerContext context)
    {
        if (m_role.RestrictedConfiguration.TryGetPrivateValue(request.Key, out SecureString secureString))
        {
            // Convert the secure string to a string
            string value = ConvertSecureStringToString(secureString);
            // Handle the value change
        }

        return Task.FromResult(new VoidResponse());
    }

    protected override void OnPluginStart()
    {
        ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            m_disposable?.Dispose();
        }
    }

    protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
    {
        // This sample plugin does not handle queries
    }

    private string ConvertSecureStringToString(SecureString secureString)
    {
        IntPtr valuePtr = IntPtr.Zero;
        try
        {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(valuePtr);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
        }
    }
}