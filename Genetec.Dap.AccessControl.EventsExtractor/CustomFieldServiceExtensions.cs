// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Linq;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities.CustomFields;

    public static class CustomFieldServiceExtensions
    {
        public static bool TryGetCustomField(this ICustomFieldService customFieldService, string name, EntityType entityType, out CustomField value)
        {
            value = customFieldService.CustomFields.FirstOrDefault(customField => customField.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && customField.EntityType == entityType);
            return value != null;
        }
    }
}
