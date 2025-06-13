// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

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