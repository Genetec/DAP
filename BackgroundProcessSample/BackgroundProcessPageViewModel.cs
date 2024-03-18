// Copyright (C) 2022 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Threading;
    using Genetec.Sdk;
    using Genetec.Sdk.Workspace.Services;
    using Prism.Commands;
    using Prism.Mvvm;

    public class BackgroundProcessPageViewModel : BindableBase
    {
        private IBackgroundProcessNotification m_selectedProcessNotification;

        private readonly Dispatcher m_dispatcher = Dispatcher.CurrentDispatcher;

        public BackgroundProcessPageViewModel(IBackgroundProcessNotificationService service)
        {
            service.CollectionChanged += (sender, args) =>
            {
                m_dispatcher.Invoke(() =>
                {
                    foreach (IBackgroundProcessNotification removed in Notifications.Where(n => args.RemovedItems.Contains(n.Id)).ToList())
                    {
                        Notifications.Remove(removed);
                    }

                    foreach (Guid added in args.AddedItems)
                    {
                        Notifications.Add(service.GetNotification(added));
                    }
                });
            };

            service.ProcessStatusChanged += (sender, args) => Update(args.Id);

            SendNotification = new DelegateCommand<string>(service.Notify, text => !string.IsNullOrEmpty(text));

            AddProcess = new DelegateCommand<string>(name => service.AddProcess(name, null));

            UpdateProgress = new DelegateCommand<decimal?>(process =>
            {
                service.UpdateProgress(SelectedNotification.Id, decimal.ToDouble(process.Value));
                Update(SelectedNotification.Id);
            }, progress => SelectedNotification != null).ObservesProperty(() => SelectedNotification);

            UpdateMessage = new DelegateCommand<string>(message =>
            {
                service.UpdateProgress(SelectedNotification.Id, message);
                Update(SelectedNotification.Id);
            }, message => SelectedNotification != null).ObservesProperty(() => SelectedNotification);

            EndProcess = new DelegateCommand<BackgroundProcessResult?>(result => service.EndProcess(SelectedNotification.Id, result.Value), result => SelectedNotification != null).ObservesProperty(() => SelectedNotification);

            ClearProcess = new DelegateCommand(() => service.ClearProcess(SelectedNotification.Id), () => SelectedNotification != null).ObservesProperty(() => SelectedNotification);

            ClearCompletedProcesses = new DelegateCommand(service.ClearCompletedProcesses);

            void Update(Guid id)
            {
                IBackgroundProcessNotification item = Notifications.FirstOrDefault(notification => notification.Id == id);
                if (item != null)
                {
                    Notifications.Remove(item);
                }

                Notifications.Add(service.GetNotification(id));
            }
        }

        public DelegateCommand<string> AddProcess { get; }

        public DelegateCommand ClearCompletedProcesses { get; }

        public DelegateCommand ClearProcess { get; }

        public DelegateCommand<BackgroundProcessResult?> EndProcess { get; }

        public DelegateCommand<string> SendNotification { get; }

        public DelegateCommand<string> UpdateMessage { get; }

        public DelegateCommand<decimal?> UpdateProgress { get; }

        public ObservableCollection<IBackgroundProcessNotification> Notifications { get; } = new ObservableCollection<IBackgroundProcessNotification>();

        public IBackgroundProcessNotification SelectedNotification
        {
            get => m_selectedProcessNotification;
            set => SetProperty(ref m_selectedProcessNotification, value);
        }
    }
}
