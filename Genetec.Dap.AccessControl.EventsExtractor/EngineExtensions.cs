// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.Queries;

    public static class EngineExtensions
    {
        public static async Task LoadEntities(this IEngine engine, IEnumerable<Guid> entities)
        {
            foreach (var guids in entities.Split(1000))
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityGuids.AddRange(guids);
                query.DownloadAllRelatedData = true;
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
        }
    }
}


