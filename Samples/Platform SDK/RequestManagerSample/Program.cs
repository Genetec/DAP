// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Workflows;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    // Create two engines for sender and receiver
    using var sender = new Engine();
    using var receiver = new Engine();

    // Add request handler to receiver
    receiver.RequestManager.AddRequestHandler<Request, Response>(OnRequestReceived);

    // Log on both clients
    ConnectionStateCode[] states = await Task.WhenAll(sender.LogOnAsync(server, username, password), receiver.LogOnAsync(server, username, password));

    // Check if both clients are successfully logged on
    if (states.All(state => state == ConnectionStateCode.Success))
    {
        Console.WriteLine($"Sending request from {sender.Client.Name} ({sender.Client.Guid}) to {receiver.Client.Name} ({receiver.Client.Guid})");

        // Send request and wait for response asynchronously
        Response response = await sender.RequestManager.SendRequestAsync<Request, Response>(
            recipientId: receiver.Client.Guid, // Receiver's client GUID
            request: new Request { Message = "PING" } // Request instance
        );

        Console.WriteLine($"Response received: {response.Reply}");
    }
    else
    {
        Console.WriteLine($"Logon failed: {string.Join(", ", states)}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    // Request handler
    void OnRequestReceived(Request request, RequestCompletion<Response> completion)
    {
        Console.WriteLine($"Request received from {receiver.GetEntity(completion.SourceApplication).Name} ({completion.SourceApplication}): {request.Message}");
        
        // Send response
        completion.SetResponse(new Response { Reply = "PONG" });
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
