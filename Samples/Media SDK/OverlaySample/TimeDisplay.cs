namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Media.Overlay;

public class TimeDisplay
{
    public void Draw(Layer layer)
    {
        layer.DrawText(DateTime.Now.ToString("F"), "Arial", 32, "Red", 30, 30);
    }
}