// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Windows.Media.Imaging;
    using Genetec.Sdk.Workspace.Tasks;

    public class NotepadTask : Task
    {
        public NotepadTask()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/TaskSample;Component/Resources/Icon.png", UriKind.RelativeOrAbsolute)); 
            Thumbnail = new BitmapImage(new Uri("pack://application:,,,/TaskSample;Component/Resources/Thumbnail.png", UriKind.RelativeOrAbsolute));
            Name = "Launch notepad";
            Description = "Launch Notepad application. This sample illustrates a Task that executes without opening a page.";
            CategoryId = new Guid(TaskCategories.Administration);
        }
            
        public override bool CanExecute()
        {
            // This task can be executed at any time.
            return true;
        }

        public override void Execute()
        {  
            HideHomePageAfterExecution = false;  // Do not hide the home page after the task is executed.
            // This task will launch the Notepad application.
            Process.Start("notepad.exe");
        }
    }
}
