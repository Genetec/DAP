// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
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
        public override IList<Field> Fields { get; } = new List<Field>
        {
            new(FieldName.Text, typeof(string)) { Title = Resources.Text, IsDisplayable = true },
            new(FieldName.Numeric, typeof(int)) { Title = Resources.Text, IsDisplayable = true },
            new(FieldName.Boolean, typeof(bool)) { Title = Resources.Text, IsDisplayable = true },
            new(FieldName.Decimal, typeof(decimal)) { Title = Resources.Text, IsDisplayable = true },
            new(FieldName.DateTime, typeof(DateTime)) { Title = Resources.Text, IsDisplayable = true }
        };

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
}