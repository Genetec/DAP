// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Sdk.Entities;
    using Sdk.Workspace.Options;

    public sealed class SampleOptionsExtensions : OptionsExtension
    {
        public static readonly Guid SampleOptionsSettings = new Guid("4B329744-E082-43F8-8B9D-9043C90D6EEB");

        public const string ExtensionName = nameof(SampleOptionsExtensions);

        public string Text
        {
            get => (string)this[nameof(Text)];
            set => this[nameof(Text)] = value;
        }

        public int Number
        {
            get => (int)this[nameof(Number)];
            set => this[nameof(Number)] = value;
        }

        public DateTime DateTime
        {
            get => (DateTime)this[nameof(DateTime)];
            set => this[nameof(DateTime)] = value;}

        public Color Color
        {
            get => (Color)this[nameof(Color)];
            set => this[nameof(Color)] = value;
        }

        public override ImageSource Icon => new BitmapImage(new Uri("pack://application:,,,/OptionsExtensionSample;component/Resources/Icon.png", UriKind.RelativeOrAbsolute));

        public override string Name => ExtensionName;

        public override string Title => Properties.Resources.SampleOptionsTitle;

        public SampleOptionsExtensions()
        {
            RegisterProperty(nameof(Text), typeof(string), default(string));
            RegisterProperty(nameof(Number), typeof(int), default(int));
            RegisterProperty(nameof(DateTime), typeof(DateTime), default(DateTime));
            RegisterProperty(nameof(Color), typeof(Color), default(Color));
        }

        protected override void Initialize()
        {
            AddOptionPage(new SampleOptionPage(this));
        }

        protected override void Load()
        {
            User user = Workspace.Sdk.LoggedUser;
            if (user != null)
            {
                SampleOptionsData data = SampleOptionsData.Deserialize(user.Settings[SampleOptionsSettings]);
                Text = data.Text;
                Number = data.Number;
                DateTime = data.DateTime;
                Color = data.Color;
            }
        }

        protected override void Save()
        {
            User user = Workspace.Sdk.LoggedUser;
            if (user != null)
            {
                var data = new SampleOptionsData
                {
                    Text = Text,
                    Number = Number,
                    DateTime = DateTime,
                    Color = Color
                };
                user.Settings[SampleOptionsSettings] = data.Serialize();
            }
        }
    }
}