// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomIcons;

namespace Genetec.Dap.CodeSamples;

public class CustomIconSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
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
}