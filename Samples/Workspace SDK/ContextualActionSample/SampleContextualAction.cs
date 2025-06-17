// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Sdk.Entities;
using Sdk.Workspace.ContextualAction;

public class SampleContextualAction : ContextualAction
{
    public SampleContextualAction()
    {
        Name = "Sample contextual action";
        Icon = new BitmapImage(new Uri("pack://application:,,,/ConfigPageSample;component/Resources/Images/SmallLogo.png"));
    }

    public override Guid Group => ContextualActionGroupId.SampleContextualActionGroupId;

    public override Guid Id => new("DB103519-8867-4809-A549-305143F0BEFF"); // TODO: Replace this GUID with your own contextual action GUID.

    public override bool CanExecute(ContextualActionContext context)
    {
        return context is ConfigurationContextualActionContext configContext
               && configContext.SelectedEntities.Select(Workspace.Sdk.GetEntity).All(ValidateEntity);
    }

    public override bool Execute(ContextualActionContext context)
    {
        if (context is not ConfigurationContextualActionContext configContext)
            return false;

        IEnumerable<Entity> entities = configContext.SelectedEntities.Select(Workspace.Sdk.GetEntity).Where(ValidateEntity);

        // TODO: Implement your action logic here
        // Example: Process each selected entity
        foreach (Entity entity in entities)
        {
            // Add your custom logic here
        }

        return true;
    }

    private bool ValidateEntity(Entity entity)
    {
        return entity is CustomEntity customEntity && customEntity.CustomEntityType == CustomEntityTypeId.Id;
    }
}
