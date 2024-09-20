// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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