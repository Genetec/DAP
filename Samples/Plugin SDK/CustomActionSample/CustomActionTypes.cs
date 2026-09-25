// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

// The CustomActionTypes class contains the unique identifiers for the custom action types used in the sample.
public static class CustomActionTypes
{
    // TODO: Replace with your own unique custom action type GUID
    public static Guid LaunchEncoderCommand { get; } = new("CD273EDD-97F6-4F1F-A1B4-3A3145801323");

    // TODO: Replace with your own unique custom action type GUID
    public static Guid SendHttpRequest { get; } = new("D8802B26-F35D-4872-99F4-EC51344A51C8");
}
