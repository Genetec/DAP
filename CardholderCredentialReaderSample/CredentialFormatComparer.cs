// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

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