using System;
using System.Collections.Generic;
using System.Windows;
using Genetec.Sdk.Workspace.Monitors.Notifications;

namespace Genetec.Dap.CodeSamples
{
    public sealed class SampleNotification : Notification
    {
        private SampleNotificationView m_view;

        protected override bool HasPrivileges => false;

        public override string Name => "Sample Notification";

        public override UIElement View => m_view ?? (m_view = new SampleNotificationView());

        protected override IList<NotificationTrayBehavior> SupportedBehaviors => new List<NotificationTrayBehavior>
        {
            NotificationTrayBehavior.Hide,
            NotificationTrayBehavior.Show,
            NotificationTrayBehavior.ShowNotificationsOnly
        };

        protected override Guid UniqueId { get; } = new Guid("21AA0F09-E35D-4C4D-B2FD-684C1877BC39");

        public SampleNotification()
        {
            Priority = 1000;
        }
    }
}