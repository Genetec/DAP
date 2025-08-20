// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Entities.AccessControl;

class AccessControlUnitManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role);

        // Retrieve the access manager role from the entity cache
        AccessManagerRole accessManagerRole = engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>().FirstOrDefault();

        var info = new AddAccessControlUnitInfo(
            address: IPAddress.Parse("127.0.0.1"),
            extensionType: AccessControlExtensionType.CloudLink,
            port: 80,
            username: "admin",
            password: new SecureString());

        engine.AccessControlUnitManager.UnitEnrollmentSucceeded += (sender, e) => Console.WriteLine("Unit enrollment succeeded");
        engine.AccessControlUnitManager.UnitEnrollmentFailed += (sender, e) => Console.WriteLine($"Unit enrollment failed: {e.ActionDetails}");
        engine.AccessControlUnitManager.UnitEnrollmentUpdated += (sender, e) => Console.WriteLine($"Unit enrollment progress: {e.State}");

        Console.WriteLine("Enrolling access control unit...");
        engine.AccessControlUnitManager.EnrollAccessControlUnit(info, accessManagerRole.Guid);
    }
}