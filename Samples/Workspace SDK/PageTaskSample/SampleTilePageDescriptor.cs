// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Genetec.Sdk.Workspace.Tasks;
using Sdk.Workspace.Pages;

public class SampleTilePageDescriptor : PageDescriptor
{
    // The name of the page as it appears in the Tasks view
    // This should be concise yet descriptive
    public override string Name => "Tile page sample";

    // A unique identifier for this page type
    // This GUID should be unique across all page types in the application
    // TODO: Generate a new GUID using a tool like Visual Studio's Create GUID tool
    public override Guid Type { get; } = new("6824D39C-5FDC-4212-972C-45DE2B263CCD");

    // A brief description of the page's purpose and functionality
    // This appears in the Tasks view and helps users understand what the page does
    public override string Description => "This sample demonstrates a tile page with camera feeds.";

    // Specifies the category of this page in the Tasks view
    // Using the Operation category (defined in TaskCategories) for tasks related to day-to-day system operations
    public override Guid CategoryId { get; } = Guid.Parse(TaskCategories.Operation);

    // Sets the color scheme for the page's icon in the Tasks view
    // VideoIconColor is appropriate for pages dealing with video feeds or camera operations
    public override TaskIconColor IconColor => TaskIconColor.VideoIconColor;

    // Determines if this page can be accessed when the user is offline
    // Set to false as this page requires live camera feeds
    public override bool AllowOfflineExecution => false;

    // Checks if the current user has the necessary privileges to access this page
    // This method should implement proper access control logic based on your system's security model
    public override bool HasPrivilege()
    {
        // TODO: Implement proper privilege checking logic
        return m_sdk.SecurityManager.IsPrivilegeGranted(CustomPrivileges.SampleTilePagePrivilege);
    }
}
