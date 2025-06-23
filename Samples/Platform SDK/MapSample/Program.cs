// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    var filePath = Path.Combine(Environment.CurrentDirectory, "OfficeFloor.png");

    var officeArea = (Area)engine.CreateEntity("Office", EntityType.Area);
    Map officeMap = officeArea.CreateMap(new DocumentMapCreationInfo(filePath: filePath, documentType: MapDocumentTypes.Image, zoomLevel: 1));

    var singaporeArea = (Area)engine.CreateEntity("Singapore", EntityType.Area);
    Map singaporeMap = singaporeArea.CreateMap(new GeographicalMapCreationInfo(MapProviders.Bing));
    singaporeMap.DefaultView = new GeoView(new GeoCoordinate(1.3521, 103.8198, 15.0), 1);
}
