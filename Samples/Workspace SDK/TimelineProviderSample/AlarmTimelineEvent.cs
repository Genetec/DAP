// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Sdk;
    using Sdk.Workspace.Components.TimelineProvider;

    public class AlarmTimelineEvent : TimelineEvent
    {
        private static readonly Size s_size = new Size(16, 16);

        private static readonly BitmapImage s_image;

        static AlarmTimelineEvent()
        {
            s_image = EntityType.Alarm.GetIcon();
            s_image.Freeze();
        }

        public AlarmTimelineEvent(Guid alarmGuid, int instanceId, DateTime dateTime) : base(dateTime)
        {
            AlarmGuid = alarmGuid;
            InstanceId = instanceId;
        }

        public Guid AlarmGuid { get; }

        public int InstanceId { get; }

        public override TimelineVisual GetVisual(Rect constraint, double msPerPixel)
        {
            var drawingVisual = new DrawingVisual();

            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawImage(s_image, new Rect(new Point(constraint.X, (constraint.Height - 16) / 2), s_size));

            drawingContext.Close();

            return new TimelineVisual(drawingVisual)
            {
                AlignmentY = AlignmentY.Center,
                AlignmentX = AlignmentX.Center
            };
        }
    }
}