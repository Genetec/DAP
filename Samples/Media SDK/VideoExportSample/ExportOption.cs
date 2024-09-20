// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using Sdk.Media.Export;

public struct ExportOption
{
    public VideoExportFormat Format { get; set; }

    public PlaybackMode PlaybackMode { get; set; }

    public bool IncludeWatermark { get; set; }

    public bool ExportAudio { get; set; }
}