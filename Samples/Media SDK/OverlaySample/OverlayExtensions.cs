// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Threading.Tasks;
using Sdk.Media.Overlay;

public static class OverlayExtensions
{
    public static async Task WaitUntilReadyForUpdate(this Overlay overlay)
    {
        var completion = new TaskCompletionSource<object>();

        overlay.StateChange += OnStateChange;
        try
        {
            if (overlay.IsReadyForUpdate)
            {
                return;
            }

            await completion.Task;
        }
        finally
        {
            overlay.StateChange -= OnStateChange;
        }

        void OnStateChange(object sender, OverlayStatusEventArgs e)
        {
            if (e.CanPropagateUpdate)
            {
                completion.SetResult(null);
            }
        }
    }
}
