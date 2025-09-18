// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Workflows;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Enter the username for Security Center authentication.
    const string password = "";         // Provide the corresponding password for the specified username.

    // Create two engines for sender and receiver
    using var sender = new Engine();
    sender.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Sender connection status changed: {args.Status}");
    sender.LoginManager.LoggedOn += (sender, e) => Console.WriteLine($"Sender logged on to server '{e.ServerName}' as user '{e.UserName}'");
    sender.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Sender logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

    using var receiver = new Engine();
    receiver.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Receiver connection status changed: {args.Status}");
    receiver.LoginManager.LoggedOn += (sender, e) => Console.WriteLine($"Receiver logged on to server '{e.ServerName}' as user '{e.UserName}'");
    receiver.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Receiver logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

    // Add request handler to receiver
    receiver.RequestManager.AddRequestHandler<Request, Response>(OnRequestReceived);

    // Set up cancellation support to allow graceful shutdown via Ctrl+C
    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += OnCancelKeyPress;

    Console.WriteLine($"Logging in to {server}... Press Ctrl+C to cancel");

    // Log on both clients
    ConnectionStateCode[] states = await Task.WhenAll(sender.LogOnAsync(server, username, password, cancellationTokenSource.Token), receiver.LogOnAsync(server, username, password, cancellationTokenSource.Token));

    // Check if both clients are successfully logged on
    if (!states.All(state => state == ConnectionStateCode.Success))
    {
        Console.WriteLine($"Logon failed: {string.Join(", ", states)}");
        return;
    }

    Console.WriteLine($"Sender:   {sender.Client.Name} ({sender.Client.Guid})");
    Console.WriteLine($"Receiver: {receiver.Client.Name} ({receiver.Client.Guid})");
    Console.WriteLine(new string('-', 40));

    while (!cancellationTokenSource.IsCancellationRequested)
    {
        Console.Write("Enter a message to send:");
        var message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message))
        {
            continue;
        }

        try
        {
            // Send request and wait for response asynchronously
            Response response = await sender.RequestManager.SendRequestAsync<Request, Response>(
                recipientId: receiver.ClientGuid, // Receiver's client GUID
                request: new Request { Message = message } // Request instance
            );

            Console.WriteLine($"Response sent from {receiver.Client.Name} ({receiver.ClientGuid}) at {DateTime.Now}: {response.Reply}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending request: {ex.Message}");
        }
    }

    // Request handler
    void OnRequestReceived(Request request, RequestCompletion<Response> completion)
    {
        Console.WriteLine($"\nRequest received from {receiver.GetEntity(completion.SourceApplication).Name} ({completion.SourceApplication}) at {completion.RequestContext.ReceptionTimestamp.ToLocalTime()}: {request.Message}");

        // Send response
        completion.SetResponse(new Response { Reply = request.Message });
    }

    void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("\nCancelling...");
        cancellationTokenSource.Cancel();
        e.Cancel = true; // Prevent immediate termination
    }
}

[DataContract]
public class Request
{
    [DataMember]
    public string Message { get; set; }
}

[DataContract]
public class Response
{
    [DataMember]
    public string Reply { get; set; }
}
