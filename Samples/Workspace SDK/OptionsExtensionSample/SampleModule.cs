// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk;
    using Sdk.Workspace.Modules;

    public class SampleModule : Module
    {
        static SampleModule() => AssemblyResolver.Initialize();

        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
            {
                var extensions = new SampleOptionsExtensions();
                extensions.Initialize(Workspace);
                Workspace.Options.Register(extensions);
            }
        }

        public override void Unload()
        {
        }
    }
}
