﻿// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Newtonsoft.Json;

    public class CustomEventPayload
    {
        public string Text { get; set; }
        public int Numeric { get; set; }
        public bool Boolean { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }

        public string Serialize() => JsonConvert.SerializeObject(this);

        public static bool TryDeserialize(string json, out CustomEventPayload payload)
        {
            try
            {
                payload = JsonConvert.DeserializeObject<CustomEventPayload>(json);
                return true;
            }
            catch
            {
                payload = null;
                return false;
            }
        }
    }
}