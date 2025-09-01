// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Entities.CustomFields;
using Sdk.Queries;

public record ArchivedVisitorData
{
    public Guid Guid { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string EmailAddress { get; init; }
    public DateTime ActivationDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public Guid Thumbnail { get; init; }
    public DateTime CheckinDate { get; init; }
    public Guid Picture { get; init; }
    public DateTime CheckoutDate { get; init; }
    public string Description { get; init; }
    public Guid Escort { get; init; }
    public Guid Escort2 { get; init; }
    public bool MandatoryEscort { get; init; }
    public DateTime VisitDate { get; init; }
    public string MobilePhoneNumber { get; init; }
    public VisitorState VisitorState { get; init; }
    public CustomFieldsCollection CustomFields { get; init; }
}