namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
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

        Console.WriteLine($"Logging to {server}... Press Ctrl+C to cancel");

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
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);

        void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancelling...");
            cancellationTokenSource.Cancel();
            Console.CancelKeyPress -= OnCancelKeyPress;
        }
    }

    protected abstract Task RunAsync(Engine engine, CancellationToken token);
}
