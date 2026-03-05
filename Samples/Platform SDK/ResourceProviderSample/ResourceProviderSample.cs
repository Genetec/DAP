// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using Genetec.Sdk;
using Genetec.Sdk.Helpers;

namespace Genetec.Dap.CodeSamples;

public class ResourceProviderSample
{
    public static void Run()
    {
        SdkResolver.Initialize();

        Console.WriteLine("Using ResourceProvider.GetStringFromEnum to get localized entity type names:\n");

        Console.WriteLine($"{"EntityType",-25} | {"Localized Name"}");
        Console.WriteLine(new string('-', 55));

        foreach (EntityType entityType in Enum.GetValues(typeof(EntityType)).OfType<EntityType>().OrderBy(type => type.ToString()))
        {
            if (entityType == EntityType.None)
                continue;

            string localizedName = ResourceProvider.GetStringFromEnum(entityType);

            if (string.IsNullOrEmpty(localizedName))
            {
                Console.WriteLine($"{entityType,-25} | (empty)");
            }
            else
            {
                Console.WriteLine($"{entityType,-25} | {localizedName}");
            }
        }
    }
}
