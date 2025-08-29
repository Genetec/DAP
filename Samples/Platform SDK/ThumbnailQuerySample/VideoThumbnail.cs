namespace Genetec.Dap.CodeSamples;

using System;

public record VideoThumbnail
{
    public Guid Camera { get; init; }
    public byte[] Data { get; init; }
    public DateTime Timestamp { get; init; }
    public DateTime LatestFrame { get; init; }
    public Guid Context { get; init; }
}