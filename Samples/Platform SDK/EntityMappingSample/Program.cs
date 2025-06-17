// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await LoadCameras();

                foreach (Camera camera in engine.GetEntities(EntityType.Camera).OfType<Camera>())
                {
                    if (!TryGetMapping(camera, out string data))
                    {
                        SaveMapping(camera, "DATA");
                    }

                    Console.WriteLine($"{camera.Name,-20}: {data}");
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadCameras()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.DownloadAllRelatedData = true;
                query.EntityTypeFilter.Add(EntityType.Camera);
                query.EntityTypeFilter.Add(EntityType.VideoUnit);
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }

            void SaveMapping(Camera camera, string data)
            {
                if (engine.GetEntity(camera.Unit) is VideoUnit unit)
                {
                    List<EntityMapping> mappings = unit.ArchiverRole.EntityMappings;

                    EntityMapping mapping = mappings.FirstOrDefault(mapping => mapping.LocalEntityId == camera.Guid);

                    if (mapping is null)
                    {
                        mappings.Add(new EntityMapping { LocalEntityId = camera.Guid, RoleId = unit.ArchiverRoleGuid, XmlInfo = data });
                    }
                    else
                    {
                        mapping.XmlInfo = data;
                    }

                    unit.ArchiverRole.EntityMappings = mappings;
                }
            }

            bool TryGetMapping(Camera camera, out string data)
            {
                if (engine.GetEntity(camera.Unit) is VideoUnit unit)
                {
                    EntityMapping mapping = unit.ArchiverRole.EntityMappings.FirstOrDefault(mapping => mapping.LocalEntityId == camera.Guid);

                    if (mapping != null)
                    {
                        data = mapping.XmlInfo;
                        return true;
                    }
                }

                data = default;
                return false;
            }
        }
    }
}
