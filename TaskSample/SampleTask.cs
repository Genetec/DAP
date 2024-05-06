// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Windows.Media.Imaging;
    using Sdk;
    using Genetec.Sdk.Workspace.Tasks;

    public class NotepadTask : Task
    {
        public NotepadTask()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/TaskSample;Component/Resources/Icon.png", UriKind.RelativeOrAbsolute)); 
            Thumbnail = new BitmapImage(new Uri("pack://application:,,,/TaskSample;Component/Resources/Thumbnail.png", UriKind.RelativeOrAbsolute));
            Name = "Launch notepad";
            Description = "Launch Notepad application. This sample illustrates a Task that executes without opening a page in the Security Desk or Config Tool.";
            CategoryId = new Guid(TaskCategories.Administration);
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override void Execute()
        {
            HideHomePageAfterExecution = false;
            Process.Start("notepad.exe");
        }
    }
}