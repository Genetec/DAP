// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Tasks;
    using System;

    public class SampleModule : Sdk.Workspace.Modules.Module
    {
        public override void Load()
        {           
            RegisterTask(new TaskGroup(CustomTaskCategories.SdkSamples, Guid.Empty, "SDK Samples", null, int.MaxValue));
            RegisterTask(new CreatePageTask<StylesSamplePage>(isSingleton: true));
            RegisterTask(new CreatePageTask<ControlsSamplePage>(isSingleton: true));
            RegisterTask(new CreatePageTask<ChartSamplePage>(isSingleton: true));
            RegisterTask(new CreatePageTask<WebBrowserSamplePage>(isSingleton: false));
            
            //Initialize the task and register it
            void RegisterTask(Task task)
            {
                task.Initialize(Workspace);
                Workspace.Tasks.Register(task);
            }
        }

        public override void Unload()
        {
        }
    }
}
