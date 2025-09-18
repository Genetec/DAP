// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk;

public record HardwareInventoryInfo
{
    public string UnitName { get; init; }
    public EntityType? UnitType { get; init; }
    public string Manufacturer { get; init; }
    public string ProductType { get; init; }
    public Guid? Role { get; init; }
    public string FirmwareVersion { get; init; }
    public string IpAddress { get; init; }
    public string PhysicalAddress { get; init; }
    public string TimeZone { get; init; }
    public string User { get; init; }
    public string PasswordStrength { get; init; }
    public string UpgradeStatus { get; init; }
    public string NextUpgrade { get; init; }
    public UnitUpgradeErrorDetails? ReasonForUpgradeFailure { get; init; }
    public State? State { get; init; }
    public string PlatformVersion { get; init; }
    public string LastUpdatePassword { get; init; }
    public string UpgradeProgression { get; init; }
    public string ReaderType { get; init; }
    public DeviceReaderEncryptionStatus? ReaderEncryptionStatus { get; init; }
    public string Related { get; init; }
    public string LicenseConsumption { get; init; }
}