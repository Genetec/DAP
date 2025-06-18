using System;
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomIcons;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server: server, username: username, password: password);

    if (state == ConnectionStateCode.Success)
    {
        var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

        CustomIconCollection customIcons = config.CustomIcons;
        Console.WriteLine($"{customIcons.Count} custom icons found.");

        foreach (CustomIcon customIcon in customIcons)
        {
            Console.WriteLine($"Icon ID: {customIcon.Id}");
            Console.WriteLine($"Entity Type: {customIcon.EntityType}");

            string iconPath = Path.Combine(Path.GetTempPath(), $"{customIcon.Id}.png");
            customIcon.Download(iconPath);
            Console.WriteLine($"Icon downloaded to: {iconPath}");

            Console.WriteLine(new string('-', 30));
        }

        Entity entity = engine.CreateEntity("Area with custom icon", EntityType.Area);
        Console.WriteLine($"Created new Area entity: {entity.Name}");

        Console.WriteLine("Adding custom icon");
        Guid iconId = entity.AddCustomIconToDirectory(Resources.Icon, true);
        
        CustomIcon icon = config.CustomIcons.Single(customIcon => customIcon.Id == iconId);
        Console.WriteLine($"Icon ID: {icon.Id}");
        Console.WriteLine($"Entity Type: {icon.EntityType}");

        Image image = entity.GetCustomIconFromDirectory(iconId, true);
        Console.WriteLine($"Retrieved icon size: {image.Width}x{image.Height}");

        entity.SetCustomIcon(iconId);
        Console.WriteLine($"Set custom icon {iconId} for entity {entity.Name}");

        entity.ResetCustomIcon(removeIconFromDirectory: false);
        Console.WriteLine($"Custom icon reset for {entity.Name}");

        entity.RemoveCustomIconFromDirectory(iconId);
        Console.WriteLine("Custom icon removed from Directory");
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}
