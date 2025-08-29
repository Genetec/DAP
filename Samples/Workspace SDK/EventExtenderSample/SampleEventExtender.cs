// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using Properties;
using Sdk.Events;
using Sdk.Workspace.Extenders;
using Sdk.Workspace.Fields;

public class SampleEventExtender : EventExtender
{
    /// <summary>
    /// Defines the custom fields to be added in the Monitoring task.
    /// </summary>
    public override IList<Field> Fields { get; } =
    [
        new(FieldName.Text, typeof(string)) { Title = Resources.Text, IsDisplayable = true },
        new(FieldName.Numeric, typeof(int)) { Title = Resources.Number, IsDisplayable = true },
        new(FieldName.Boolean, typeof(bool)) { Title = Resources.Boolean, IsDisplayable = true },
        new(FieldName.Decimal, typeof(decimal)) { Title = Resources.Decimal, IsDisplayable = true },
        new(FieldName.DateTime, typeof(DateTime)) { Title = Resources.DateTIme, IsDisplayable = true }
    ];

    public override bool Extend(Event @event, FieldsCollection fields)
    {
        // Check if the event is a CustomEventInstance and try to deserialize its payload
        if (@event is CustomEventInstance instance && CustomEventPayload.TryDeserialize(instance.ExtraHiddenPayload, out CustomEventPayload payload))
        {
            // Populate the custom fields with data from the payload
            fields[FieldName.Text] = payload.Text;
            fields[FieldName.Numeric] = payload.Numeric;
            fields[FieldName.Boolean] = payload.Boolean;
            fields[FieldName.Decimal] = payload.Decimal;
            fields[FieldName.DateTime] = payload.DateTime;
            return true;
        }
        return false;
    }
}