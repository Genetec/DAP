// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.EventsArgs;
using Sdk.Workflows.EntityManager;

static class EntityManagerExtensions
{
    public static async Task<CopyConfigResultEventArgs> CopyConfigurationAsync(this IEntityManager manager, Guid source, IEnumerable<Guid> destinations, IProgress<int> progress = null, CancellationToken cancellationToken = default, params CopyConfigOption[] options)
    {
        var taskCompletionSource = new TaskCompletionSource<CopyConfigResultEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

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
            // Cancellation stops the wait; the SDK copy operation may continue.
            using CancellationTokenRegistration registration = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
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
