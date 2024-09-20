// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Services;
using Prism.Commands;
using Prism.Mvvm;
using static System.String;

public class BackgroundProcessPageViewModel : BindableBase
{
    private readonly IBackgroundProcessNotificationService m_service;
        
    private string m_notificationMessage;

    public BackgroundProcessPageViewModel(IBackgroundProcessNotificationService service)
    {
        m_service = service;
        StartProcessCommand = new DelegateCommand(StartProcess);
        SendNotificationCommand = new DelegateCommand(() => m_service.Notify(NotificationMessage), () => !IsNullOrEmpty(NotificationMessage)).ObservesProperty(() => NotificationMessage);
    }

    public DelegateCommand StartProcessCommand { get; }

    public DelegateCommand SendNotificationCommand { get; }

    public string NotificationMessage
    {
        get => m_notificationMessage;
        set => SetProperty(ref m_notificationMessage, value);
    }

    private async void StartProcess()
    {
        Guid processId = m_service.AddProcess("Background process");

        for (int i = 0; i <= 100; i += 10)
        {
            await Task.Delay(500);
            m_service.UpdateProgress(processId, i);
        }

        m_service.EndProcess(processId, BackgroundProcessResult.Success, "Background process completed.");
    }
}