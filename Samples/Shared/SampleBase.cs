// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
using Genetec.Sdk.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base class for SDK samples that provides common infrastructure for connecting to Security Center.
/// This class implements the Template Method pattern, handling connection management, authentication,
/// and error handling while allowing derived classes to focus on their specific SDK demonstrations.
/// </summary>
public abstract class SampleBase
{
    /// <summary>
    /// Static constructor that initializes the SDK resolver for assembly loading.
    /// This ensures the SDK dependencies are properly resolved before any sample execution.
    /// </summary>
    static SampleBase() => SdkResolver.Initialize();

    /// <summary>
    /// Main entry point for running the sample. Handles the complete lifecycle including
    /// connection setup, authentication, sample execution, and cleanup.
    /// </summary>
    public async Task RunAsync()
    {
        // Connection parameters for your Security Center server
        // NOTE: Update these values to match your Security Center environment
        const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
        const string username = "admin";    // Enter the username for Security Center authentication.
        const string password = "";         // Provide the corresponding password for the specified username.

        // Create the SDK engine instance that provides access to Security Center APIs
        using var engine = new Engine();

        // Configure connection event handlers to provide feedback during authentication
        // These events help track the connection state and diagnose authentication issues
        engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Status changed: {args.Status}");
        engine.LoginManager.LoggedOn += (sender, e) => Console.WriteLine($"Logged on to server '{e.ServerName}' as user '{e.UserName}'");
        engine.LoginManager.LoggingOff += (sender, e) => Console.WriteLine("Logging off");
        engine.LoginManager.LoggedOff += (sender, e) => Console.WriteLine($"Logged off. AutoReconnect={e.AutoReconnect}");
        engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

        // Set up cancellation support to allow graceful shutdown via Ctrl+C
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += OnCancelKeyPress;

        Console.WriteLine($"Logging in to {server}... Press Ctrl+C to cancel");

        // Attempt to connect to Security Center using the provided credentials
        ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password, cancellationTokenSource.Token);

        if (state == ConnectionStateCode.Success)
        {
            try
            {
                // Execute the sample-specific logic implemented by the derived class
                await RunAsync(engine, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, likely due to Ctrl+C being pressed
                Console.WriteLine("Operation cancelled");
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions during sample execution
                Console.WriteLine(ex);
            }
            Console.CancelKeyPress -= OnCancelKeyPress;
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        // Only prompt for keypress if the app wasn't cancelled via Ctrl+C
        if (!cancellationTokenSource.IsCancellationRequested)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Cancelling...");
                cancellationTokenSource.Cancel();
                // Prevent the default Ctrl+C behavior (which would terminate immediately)
                e.Cancel = true;
            }
        }
    }

    protected abstract Task RunAsync(Engine engine, CancellationToken token);

    /// <summary>
    /// Loads entities of the specified types into the SDK's entity cache using proper paging.
    /// This demonstrates the recommended approach for entity loading in Security Center SDK applications.
    /// </summary>
    /// <param name="engine">The SDK engine instance</param>
    /// <param name="token">Cancellation token to allow operation cancellation</param>
    /// <param name="types">The entity types to load (e.g., EntityType.Camera, EntityType.Door)</param>
    protected async Task LoadEntities(Engine engine, CancellationToken token, params EntityType[] types)
    {
        Console.WriteLine($"Loading entities: {string.Join(", ", types)}...");

        // Create an EntityConfiguration query to load entities into the cache
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);

        // Specify which entity types to load
        query.EntityTypeFilter.AddRange(types);

        // Download all related entity data (recommended in most cases)
        query.DownloadAllRelatedData = true;

        // Use paging to handle large datasets efficiently (1000 entities per page)
        query.PageSize = 1000;
        query.Page = 1;

        int totalLoaded = 0;
        QueryCompletedEventArgs args;
        do
        {
            // Check for cancellation before each page
            token.ThrowIfCancellationRequested();

            // Execute the query asynchronously. This loads entities into the cache
            // The returned DataTable contains GUIDs of the loaded entities
            args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            totalLoaded += args.Data.Rows.Count;

            Console.WriteLine($"  Page {query.Page}: loaded {args.Data.Rows.Count} entities");

            // Move to the next page for subsequent iterations
            query.Page++;

        } while (args.Data.Rows.Count >= query.PageSize); // Continue until fewer results than page size are returned

        Console.WriteLine($"Completed loading {totalLoaded} entities total");
    }
}
