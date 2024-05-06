// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk;

    public static class AedUnitCustomPrivilege
    {
        public static SdkPrivilege View { get; } = new SdkPrivilege(new Guid("0896B350-7225-4A53-8DDC-E8365CAE456E"));

        public static SdkPrivilege Modify { get; } = new SdkPrivilege(new Guid("1938B316-B651-43B7-96B1-B94C3FDBB756"));

        public static SdkPrivilege Add { get; } = new SdkPrivilege(new Guid("0707B77A-1091-4AEB-877E-30EAA80E9626"));

        public static SdkPrivilege Delete { get; } = new SdkPrivilege(new Guid("6008ECEB-024F-4D4C-BC54-0B8BC8C58D5B"));
    }
}