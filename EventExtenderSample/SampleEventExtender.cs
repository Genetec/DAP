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
    using Sdk.Events;
    using Sdk.Workspace.Extenders;
    using Sdk.Workspace.Fields;

    public class SampleEventExtender : EventExtender
    {
        public override IList<Field> Fields { get; } = new List<Field>
        {
            new("Text", typeof(string)) { Title = "Text", IsDisplayable = true },
            new("Numeric", typeof(int)) { Title = "Numeric", IsDisplayable = true },
            new("Boolean", typeof(bool)) { Title = "Boolean", IsDisplayable = true },
            new("Decimal", typeof(decimal)) { Title = "Decimal", IsDisplayable = true },
            new("DateTime", typeof(DateTime)) { Title = "DateTIme", IsDisplayable = true }
        };

        public override bool Extend(Event @event, FieldsCollection fields)
        {
            if (@event is CustomEventInstance instance && ExtraHiddenPayload.TryDeserialize(instance.ExtraHiddenPayload, out var payload))
            {
                fields["Text"] = payload.Text;
                fields["Numeric"] = payload.Numeric;
                fields["Boolean"] = payload.Boolean;
                fields["Decimal"] = payload.Decimal;
                fields["DateTime"] = payload.DateTime;
                return true;
            }

            return false;
        }
    }
}