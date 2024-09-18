// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.EventsArgs;
    using Sdk.Workflows.EntityManager;

    static class EntityManagerExtensions
    {
        public static async Task<CopyConfigResultEventArgs> CopyConfigurationAsync(this IEntityManager manager, Guid source, IEnumerable<Guid> destinations, IProgress<int> progress = null, params CopyConfigOption[] options)
        {
            var taskCompletionSource = new TaskCompletionSource<CopyConfigResultEventArgs>();

            EventHandler<CopyConfigResultEventArgs> copyConfigFinished = (s, e) => taskCompletionSource.TrySetResult(e);
            manager.CopyConfigFinished += copyConfigFinished;

            EventHandler<CopyConfigProgressEventArgs> copyConfigProgress = null;
            if (progress != null)
            {
                copyConfigProgress = (s, e) => progress.Report(e.ProgressPercentage);
                manager.CopyConfigProgress += copyConfigProgress;
            }

            try
            {
                manager.CopyConfiguration(source, destinations.ToList(), options.ToList());
                return await taskCompletionSource.Task;
            }
            finally
            {
                manager.CopyConfigFinished -= copyConfigFinished;
                if (copyConfigProgress != null)
                {
                    manager.CopyConfigProgress -= copyConfigProgress;
                }
            }
        }
    }
}