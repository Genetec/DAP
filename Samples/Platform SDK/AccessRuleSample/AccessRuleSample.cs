// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.AccessControl.AccessRules;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Coverages;

namespace Genetec.Dap.CodeSamples;

public class AccessRuleSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        Console.WriteLine("Creating entities and configuring access rules...");

        await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            // Create a secure area
            var secureArea = (Area)engine.CreateEntity("Secure Area", EntityType.Area);
            Console.WriteLine($"Created secure area: {secureArea.Name}");

            // Create a perimeter door for entry and exit
            var perimeterDoor = (Door)engine.CreateEntity("Main Entrance", EntityType.Door);
            secureArea.AddDoor(perimeterDoor.Guid);
            Console.WriteLine($"Created perimeter door: {perimeterDoor.Name}");

            // Create a captive door within the secure area
            var captiveDoor = (Door)engine.CreateEntity("Server Room Door", EntityType.Door);
            secureArea.AddDoor(captiveDoor.Guid);
            Console.WriteLine($"Created captive door: {captiveDoor.Name}");

            // Create an access rule for the perimeter door
            AccessRule perimeterAccessRule = engine.CreateAccessRule("Perimeter Access Rule", AccessRuleType.Permanent);
            Console.WriteLine($"Created perimeter access rule: {perimeterAccessRule.Name}");

            // Create a daily coverage schedule from 8 AM to 6 PM for the perimeter door
            DailyCoverage coverage = (DailyCoverage)CoverageFactory.Instance.Create(CoverageType.Daily);
            coverage.Add(new DailyCoverageItem(new SdkTime(8, 0, 0), new SdkTime(18, 0, 0)));
            var schedule = (Schedule)engine.CreateEntity("Perimeter Access Hours", EntityType.Schedule);
            schedule.Coverage = coverage;
            perimeterAccessRule.Schedule = schedule;
            Console.WriteLine($"Created schedule: {schedule.Name}");

            // Add the perimeter access rule to the perimeter door
            perimeterDoor.AddAccessRule(perimeterAccessRule.Guid, AccessRuleSide.Both);
            Console.WriteLine($"Added access rule to perimeter door: {perimeterDoor.Name}");

            // Create an access rule for the captive door
            AccessRule captiveAccessRule = engine.CreateAccessRule("Server Room Access Rule", AccessRuleType.Permanent);
            Console.WriteLine($"Created captive access rule: {captiveAccessRule.Name}");

            // Add the captive access rule to the captive door
            captiveDoor.AddAccessRule(captiveAccessRule.Guid, AccessRuleSide.Both);
            Console.WriteLine($"Added access rule to captive door: {captiveDoor.Name}");

            // Create cardholder groups for perimeter and captive access
            var perimeterGroup = (CardholderGroup)engine.CreateEntity("Perimeter Access Group", EntityType.CardholderGroup);
            perimeterAccessRule.AddCardholders(perimeterGroup.Guid);
            Console.WriteLine($"Created perimeter cardholder group: {perimeterGroup.Name}");

            var captiveGroup = (CardholderGroup)engine.CreateEntity("Server Room Access Group", EntityType.CardholderGroup);
            captiveAccessRule.AddCardholders(captiveGroup.Guid);
            Console.WriteLine($"Created captive cardholder group: {captiveGroup.Name}");
        });

        Console.WriteLine("Configuration completed successfully.");
    }
}