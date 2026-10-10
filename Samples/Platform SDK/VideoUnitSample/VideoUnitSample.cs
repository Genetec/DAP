// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Video;
using Genetec.Sdk.Workflows;
using Genetec.Sdk.Workflows.UnitManager;

namespace Genetec.Dap.CodeSamples;

using Sdk.EventsArgs;

public class VideoUnitSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        PrintSupportedCameras(engine.VideoUnitManager);

        VideoUnitProductInfo productInfo = engine.VideoUnitManager.FindProductsByManufacturer("Generic Stream").FirstOrDefault(p => p.ProductType == "RTSP");

        if (productInfo != null)
        {
            Console.WriteLine("Enrolling video unit from Generic RTSP stream...");
            Console.Write("Enter a valid RTSP stream URI using an IP address: ");

            if (!Uri.TryCreate(Console.ReadLine(), UriKind.Absolute, out Uri uri)
                || !string.Equals(uri.Scheme, "rtsp", StringComparison.OrdinalIgnoreCase)
                || !IPAddress.TryParse(uri.Host, out IPAddress address)
                || uri.Port <= 0)
            {
                Console.WriteLine("Enter an absolute RTSP URI with an IP address and a valid port.");
                return;
            }

            // Create the AddVideoUnitInfo object without duplicating IP/port
            AddVideoUnitInfo addVideoUnitInfo = new(videoUnitProductInfo: productInfo,
                ipEndPoint: new IPEndPoint(address, uri.Port),
                useDefaultCredentials: true);

            addVideoUnitInfo.SpecialFeatures.Add("StreamUri", uri.OriginalString);

            ArchiverRole archiver = await GetArchiverRole(engine, token);
            if (archiver != null)
            {
                try
                {
                    Progress<EnrollmentResult> progress = new(result => Console.WriteLine(result));
                    Guid videoUnitId = await AddVideoUnit(engine.VideoUnitManager, addVideoUnitInfo, archiver.Guid, token, progress);
                    var videoUnit = (VideoUnit)engine.GetEntity(videoUnitId);
                    Console.WriteLine($"Video unit: {videoUnit} has been created.");
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No Archiver role was found. An Archiver role must be created to enroll a camera.");
            }
        }
    }

    private async Task<ArchiverRole> GetArchiverRole(Engine engine, CancellationToken token)
    {
        // Load all roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role);
        // Find the Archiver role in the entity cache
        return engine.GetEntities(EntityType.Role).OfType<ArchiverRole>().FirstOrDefault();
    }

    private void PrintSupportedCameras(IVideoUnitManager manager)
    {
        Console.WriteLine("Listing supported manufacturers and models");

        foreach (var grouping in ListSupportedCameras(manager).GroupBy(productInfo => productInfo.Manufacturer).OrderBy(grouping => grouping.Key))
        {
            Console.WriteLine($"Manufacturer: {grouping.Key}");
            Console.WriteLine(new string('-', 50));

            foreach (var productInfo in grouping)
            {
                Console.WriteLine($"  Product Type: {productInfo.ProductType}");
                Console.WriteLine($"  Description: {productInfo.Description}");
                Console.WriteLine("\n  Capabilities:");
                PrintProductCapability(productInfo.ProductCapability);
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }

    private void PrintProductCapability(ProductCapability capability)
    {
        if (capability == null)
            return;

        const string indent = "    ";  // 4 spaces for alignment

        Console.WriteLine($"{indent}Username: {capability.IsUsernameSupported,-6} Password: {capability.IsPasswordSupported}");

        Console.WriteLine($"{indent}Command Port:     {capability.IsCommandPortSupported,-6} (Default: {capability.DefaultCommandPort})");
        Console.WriteLine($"{indent}Secure Port:      {capability.IsSecureCommandPortSupported,-6} (Default: {capability.DefaultSecureCommandPort})");
        Console.WriteLine($"{indent}Discovery Port:   {capability.IsDiscoveryPortSupported,-6} (Default: {capability.DefaultDiscoveryPort})");

        Console.WriteLine($"{indent}IPv4: {capability.IsIPv4Supported,-6} IPv6: {capability.IsIPv6Supported,-6} Hostname: {capability.IsHostnameSupported}");
        Console.WriteLine($"{indent}Manual Add: {capability.IsManualAddSupported}");

        if (capability.SpecialFeatures.Any())
        {
            Console.WriteLine($"\n{indent}Special Features:");
            foreach (string feature in capability.SpecialFeatures)
            {
                Console.WriteLine($"{indent}- {feature}");
            }
        }
    }

    private IEnumerable<VideoUnitProductInfo> ListSupportedCameras(IVideoUnitManager videoUnitManager) => videoUnitManager.Manufacturers.SelectMany(videoUnitManager.FindProductsByManufacturer);

    private async Task<Guid> AddVideoUnit(IVideoUnitManager videoUnitManager, AddVideoUnitInfo videoUnitInfo, Guid archiver, CancellationToken token, IProgress<EnrollmentResult> progress = default)
    {
        token.ThrowIfCancellationRequested();
        var completion = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

        videoUnitManager.EnrollmentStatusChanged += OnEnrollmentStatusChanged;
        try
        {
            AddUnitResponse response = await WaitForEnrollment(videoUnitManager.AddVideoUnit(videoUnitInfo, archiver), token);

            return response.Error != Error.None ? throw new Exception($"Fail to add video unit {response.Error}") : await WaitForEnrollment(completion.Task, token);
        }
        finally
        {
            videoUnitManager.EnrollmentStatusChanged -= OnEnrollmentStatusChanged;
        }

        static async Task<T> WaitForEnrollment<T>(Task<T> task, CancellationToken cancellationToken)
        {
            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = cancellationToken.Register(() => cancellation.TrySetResult(true));

            if (await Task.WhenAny(task, cancellation.Task) != task)
            {
                // The SDK request can continue after this wait ends; observe any later fault.
                _ = task.ContinueWith(completed => _ = completed.Exception, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return await task;
        }

        void OnEnrollmentStatusChanged(object sender, UnitEnrolledEventArgs e)
        {
            if (!IsEnrollmentForRequestedUnit(e))
            {
                return;
            }

            progress?.Report(e.EnrollmentResult);

            if (e.EnrollmentResult != EnrollmentResult.Connecting)
            {
                if (e.Unit != Guid.Empty)
                {
                    completion.TrySetResult(e.Unit);
                }
                else
                {
                    completion.TrySetException(new Exception($"Unable to enroll the video unit: {e.EnrollmentResult}"));
                }
            }
        }

        bool IsEnrollmentForRequestedUnit(UnitEnrolledEventArgs e)
        {
            string expectedAddress = videoUnitInfo.Hostname ?? videoUnitInfo.IPEndPoint?.Address.ToString();
            if (!string.IsNullOrWhiteSpace(expectedAddress) && !string.Equals(e.Address, expectedAddress, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int expectedPort = videoUnitInfo.Port;
            return expectedPort < 0 || e.Port == expectedPort;
        }
    }
}
