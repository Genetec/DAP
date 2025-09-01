// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;

namespace Genetec.Dap.CodeSamples;

public static class CustomPrivileges
{
    public static Guid SamplePagePrivilege { get; } = new("FB7EF0E7-93C1-4946-A673-6FA914A34AD0"); // TODO: Replace with your own unique privilege GUID

    public static Guid SampleTilePagePrivilege { get; } = new("F9CC755D-310A-44CE-991C-C726926F9020"); // TODO: Replace with your own unique privilege GUID
}
