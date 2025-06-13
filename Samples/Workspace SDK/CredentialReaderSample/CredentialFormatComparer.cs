// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Collections.Generic;
    using Sdk.Credentials;

    public class CredentialFormatComparer : IEqualityComparer<CredentialFormat>
    {
        public bool Equals(CredentialFormat x, CredentialFormat y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return x.UniqueId == y.UniqueId;
        }

        public int GetHashCode(CredentialFormat obj) => obj.UniqueId.GetHashCode();
    }
}