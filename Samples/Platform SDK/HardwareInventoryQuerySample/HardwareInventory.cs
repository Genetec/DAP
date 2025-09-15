// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record HardwareInventory
{
    public string UnitName { get; init; }
    public EntityType UnitType { get; init; }
    public string Manufacturer { get; init; }
    public string FirmwareVersion { get; init; }
    public string IpAddress { get; init; }
    public Guid RoleGuid { get; init; }
}