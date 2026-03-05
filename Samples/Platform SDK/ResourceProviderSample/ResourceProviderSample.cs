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

        Console.WriteLine("Using ResourceProvider.GetStringFromEnum to get localized names from SDK enums.\n");
        Console.WriteLine("The following enums have ResourceReference attributes and support localized string lookup:\n");

        PrintEnum<EntityType>("EntityType");
        PrintEnum<RoleType>("RoleType");
        PrintEnum<CredentialState>("CredentialState");
        PrintEnum<StreamingType>("StreamingType");
        PrintEnum<DeviceReaderEncryptionStatus>("DeviceReaderEncryptionStatus");
    }

    private static void PrintEnum<TEnum>(string enumName) where TEnum : struct, IConvertible
    {
        Console.WriteLine($"--- {enumName} ---");
        Console.WriteLine($"{"Value",-35} | {"Localized Name"}");
        Console.WriteLine(new string('-', 60));

        foreach (TEnum value in Enum.GetValues(typeof(TEnum)).OfType<TEnum>().OrderBy(v => v.ToString()))
        {
            string localizedName = ResourceProvider.GetStringFromEnum(value);
            Console.WriteLine(string.IsNullOrEmpty(localizedName)
                ? $"{value,-35} | (empty)"
                : $"{value,-35} | {localizedName}");
        }

        Console.WriteLine();
    }
}
