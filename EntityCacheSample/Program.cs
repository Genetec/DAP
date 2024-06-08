// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
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

            engine.EntitiesAdded += OnEntitiesAdded;
            engine.EntitiesInvalidated += OnEntitiesInvalidated;
            engine.EntitiesRemoved += OnEntitiesRemoved;

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                PrintEntityCache();

                EntityType[] entityTypes = { EntityType.Role, EntityType.Server, EntityType.User, EntityType.UserGroup };

                await LoadEntitiesIntoEntityCache(entityTypes);

                PrintEntityCache();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void PrintEntityCache()
            {
                Console.WriteLine("\nCurrent Entity Cache:");

                foreach (var entityType in Enum.GetValues(typeof(EntityType)).OfType<EntityType>().Except(new[] { EntityType.None, EntityType.ReportTemplate }))
                {
                    try
                    {
                        int count = engine.GetEntities(entityType).Count;
                        if (count > 0)
                        {
                            Console.WriteLine($"{entityType,-20}: {count}");
                        }
                    }
                    catch
                    {
                    }
                }
            }

            async Task LoadEntitiesIntoEntityCache(params EntityType[] types)
            {
                Console.WriteLine("\nLoading entities into entity cache");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.AddRange(types);
                query.Page = 1;
                query.PageSize = 1000;

                QueryCompletedEventArgs args;

                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    query.Page++;
                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);
            }

            void OnEntitiesRemoved(object sender, EntitiesRemovedEventArgs e)
            {
                var removedEntities = e.Entities.Select(info => $"{info.EntityType} {info.EntityGuid}").ToList();
                if (removedEntities.Any())
                {
                    Console.WriteLine($"Entities removed ({removedEntities.Count}): {string.Join(", ", removedEntities)}");
                }
            }

            void OnEntitiesAdded(object sender, EntitiesAddedEventArgs e)
            {
                var addedEntities = e.Entities.Select(info => engine.GetEntity(info.EntityGuid)).ToList();
                if (addedEntities.Any())
                {
                    Console.WriteLine($"Entities added ({addedEntities.Count}): {string.Join(", ", addedEntities)}");
                }
            }

            void OnEntitiesInvalidated(object sender, EntitiesInvalidatedEventArgs e)
            {
                var modifiedEntities = e.Entities.Select(info => engine.GetEntity(info.EntityGuid)).ToList();
                if (modifiedEntities.Any())
                {
                    Console.WriteLine($"Entities modified ({modifiedEntities.Count}): {string.Join(", ", modifiedEntities)}");
                }
            }
        }
    }
}
