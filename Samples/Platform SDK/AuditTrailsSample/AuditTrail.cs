// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record AuditTrail
{
    public int Id { get; init; }
    public Guid EntityGuid { get; init; }
    public DateTime ModificationTimestamp { get; init; }
    public Guid ModifiedBy { get; init; }
    public Guid SourceApplicationGuid { get; init; }
    public string Name { get; init; }
    public string ModifiedByAsString { get; init; }
    public string SourceApplicationAsString { get; init; }
    public string Machine { get; init; }
    public ApplicationType SourceApplicationType { get; init; }
    public string OldValue { get; init; }
    public string NewValue { get; init; }
    public Guid CustomFieldId { get; init; }
    public string CustomFieldName { get; init; }
    public CustomFieldValueType CustomFieldValueType { get; init; }
    public AuditTrailModificationType ModificationType { get; init; }
    public EntityType EntityType { get; init; }
    public string Description { get; init; }
    public string Value { get; init; }
}