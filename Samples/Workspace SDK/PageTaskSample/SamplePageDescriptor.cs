// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

public class SamplePageDescriptor : PageDescriptor
{
    // The name of the page as it appears in the Tasks view
    public override string Name => "Page sample";

    // A unique identifier for this page type
    // TODO: Replace this with a new GUID to ensure uniqueness
    public override Guid Type { get; } = new("10B5DC08-51DF-470E-9169-8344DF69F372");

    // A brief description of the page's purpose, visible in the Tasks view
    public override string Description => "This sample demonstrate a sample page.";

    // Specifies the category of this page in the Tasks view
    // Using the Operation category (defined in TaskCategories) for tasks related to day-to-day system operations
    public override Guid CategoryId { get; } = Guid.Parse(Sdk.Workspace.Tasks.TaskCategories.Operation);

    // Sets the color scheme for the page's icon in the Tasks view
    // DefaultIconColor is used for general-purpose pages
    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

    // Determines if this page can be accessed when the user is offline
    public override bool AllowOfflineExecution => false;

    // Checks if the current user has the necessary privileges to access this page
    // Currently allows all users access; should be replaced with actual privilege checking
    public override bool HasPrivilege()
    {
        // TODO: Implement proper privilege checking logic
        return m_sdk.SecurityManager.IsPrivilegeGranted(CustomPrivileges.SamplePagePrivilege);
    }
}