// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk;
    using Genetec.Sdk.Diagnostics.Logging.Core;
    using Genetec.Sdk.Workspace.Commands;
    using Genetec.Sdk.Workspace.Modules;

    public class CommandManagerSampleModule : Module
    {
        private Logger m_logger;

        public override void Load()
        {
            m_logger = Logger.CreateInstanceLogger(this);
            
            Workspace.Commands.Evaluating += OnCommandsOnEvaluating;
            Workspace.Commands.Evaluated += OnCommandsOnEvaluated;
            Workspace.Commands.Executing += OnCommandsOnExecuting;
            Workspace.Commands.Executed += OnCommandsOnExecuted;
            Workspace.Commands.Invalidated += OnInvalidated;
        }

        private void OnCommandsOnExecuted(object sender, ExecutedEventArgs e)
        {
            m_logger.TraceDebug($"Command executed: {e.Command.Name}, Parameter {e.Parameter}");
        }

        private void OnCommandsOnExecuting(object sender, CommandCancelExecutionEventArgs e)
        {
            m_logger.TraceDebug($"Command executing: {e.Command.Name}, Parameter {e.Parameter}");
        }

        private void OnCommandsOnEvaluated(object sender, EvaluatedEventArgs e)
        {
            m_logger.TraceDebug($"Command evaluated: {e.Command.Name}, Parameter {e.Parameter}");
        }

        private void OnCommandsOnEvaluating(object sender, CommandCancelExecutionEventArgs e)
        {
            m_logger.TraceDebug($"Command evaluating: {e.Command.Name}, Parameter {e.Parameter}");
        }

        private void OnInvalidated(object sender, CommandEventArgs e)
        {
            m_logger.TraceDebug($"Command invalidated: {e.Command.Name}");
        }

        public override void Unload()
        {
            Workspace.Commands.Evaluating -= OnCommandsOnEvaluating;
            Workspace.Commands.Evaluated -= OnCommandsOnEvaluated;
            Workspace.Commands.Executing -= OnCommandsOnExecuting;
            Workspace.Commands.Executed -= OnCommandsOnExecuted;
            Workspace.Commands.Executed -= OnInvalidated;
            
            m_logger?.Dispose();
        }
    }
}
