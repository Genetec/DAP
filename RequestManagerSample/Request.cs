﻿// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Request
    {
        [DataMember]
        public string Message { get; set; }
    }
}