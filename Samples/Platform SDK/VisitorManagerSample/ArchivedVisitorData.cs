// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Entities.CustomFields;
    using Sdk.Queries;

    class ArchivedVisitorData
    {
        public Guid Guid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Guid Thumbnail { get; set; }
        public DateTime CheckinDate { get; set; }
        public Guid Picture { get; set; }
        public DateTime CheckoutDate { get; set; }
        public string Description { get; set; }
        public Guid Escort { get; set; }
        public Guid Escort2 { get; set; }
        public bool MandatoryEscort { get; set; }
        public DateTime VisitDate { get; set; }
        public string MobilePhoneNumber { get; set; }
        public VisitorState VisitorState { get; set; }
        public CustomFieldsCollection CustomFields { get; set; }
    }
}