// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk;

public record ActivityTrail
{
    public Guid InitiatorEntityId { get; init; }
    public string InitiatorEntityName { get; init; }
    public EntityType InitiatorEntityType { get; init; }
    public string InitiatorEntityTypeName { get; init; }
    public string InitiatorEntityVersion { get; init; }
    public string Description { get; init; }
    public ActivityType ActivityType { get; init; }
    public string ActivityTypeName { get; init; }
    public Guid ImpactedEntityId { get; init; }
    public string ImpactedEntityName { get; init; }
    public EntityType ImpactedEntityType { get; init; }
    public string ImpactedEntityTypeName { get; init; }
    public string InitiatorMachineName { get; init; }
    public ApplicationType InitiatorApplicationType { get; init; }
    public string InitiatorApplicationName { get; init; }
    public string InitiatorApplicationVersion { get; init; }
    public DateTime EventTimestamp { get; init; }
}