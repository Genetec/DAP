// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Prism.Commands;
using Sdk;
using Sdk.Entities;
using Sdk.Workspace.Components.CustomAction;
using Sdk.Workspace.Services;

public partial class SampleCustomActionView : CustomActionView, INotifyPropertyChanged
{
    private Camera m_camera;

    private EncoderCommandInfo m_encoderCommand;

    public SampleCustomActionView()
    {
        InitializeComponent();
        DataContext = this;

        ActionName = "Launch encoder command"; // The name of the action that will be displayed in the action list.
        ActionDescription = "Launch a video encoder command"; // The description of the action that will be displayed in the action list.

        SelectCameraCommand = new DelegateCommand(() =>
        {
            // Federated cameras are not supported for this action.
            List<Guid> cameras = Workspace.Services.Get<IDialogService>().ShowEntityBrowserDialog(new EntityTypeCollection(new[] { EntityType.Camera }), null, false, SelectionMode.Single);

            Camera = Workspace.Sdk.GetEntity(cameras.FirstOrDefault()) as Camera;
        });
    }

    public ObservableCollection<EncoderCommandInfo> EncoderCommands { get; } = new();

    public override bool IsStateValid => Camera is not null && EncoderCommand is not null;

    public EncoderCommandInfo EncoderCommand
    {
        get => m_encoderCommand;
        set
        {
            if (SetProperty(ref m_encoderCommand, value))
            {
                OnModified();
                LoadRecipients();
            }
        }
    }

    public DelegateCommand SelectCameraCommand { get; }

    public Camera Camera
    {
        get => m_camera;
        set
        {
            if (SetProperty(ref m_camera, value))
            {
                EncoderCommands.Clear();

                if (m_camera != null)
                {
                    foreach (EncoderCommandInfo commandInfo in m_camera.SupportedEncoderCommands)
                    {
                        EncoderCommands.Add(commandInfo);
                    }
                }

                EncoderCommand = EncoderCommands.FirstOrDefault();
                OnModified();
                LoadRecipients();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void LoadRecipients()
    {
        Recipients.Clear();
        foreach (Role role in Workspace.Sdk.GetEntities(EntityType.Role).OfType<Role>().Where(role => role.SubType == PluginTypes.SamplePlugin))
        {
            Recipients.Add(role.Guid);
        }
    }

    protected override string Serialize()
    {
        return new LaunchEncoderCommandAction { Camera = Camera?.Guid ?? Guid.Empty, EncoderCommand = EncoderCommand?.Id ?? 0 }.Serialize();
    }

    protected override void Deserialize(string payload)
    {
        LaunchEncoderCommandAction data = LaunchEncoderCommandAction.Deserialize(payload);
        if (data != null)
        {
            Camera = Workspace.Sdk.GetEntity(data.Camera) as Camera;
            EncoderCommand = EncoderCommands.FirstOrDefault(commandInfo => commandInfo.Id == data.EncoderCommand);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        return false;
    }
}