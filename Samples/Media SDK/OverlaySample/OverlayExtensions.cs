// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Threading;
using System.Threading.Tasks;
using Sdk.Media.Overlay;

public static class OverlayExtensions
{
    public static async Task WaitUntilReadyForUpdate(this Overlay overlay, CancellationToken cancellationToken = default)
    {
        var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        overlay.StateChange += OnStateChange;
        try
        {
            if (overlay.IsReadyForUpdate)
            {
                return;
            }

            using (cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken)))
            {
                await completion.Task;
            }
        }
        finally
        {
            overlay.StateChange -= OnStateChange;
        }

        void OnStateChange(object sender, OverlayStatusEventArgs e)
        {
            if (e.CanPropagateUpdate)
            {
                completion.TrySetResult(null);
            }
        }
    }
}
