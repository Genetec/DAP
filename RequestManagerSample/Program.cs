// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var sender = new Engine();
            using var receiver = new Engine();

            ConnectionStateCode[] states = await Task.WhenAll(sender.LogOnAsync(server, username, password), receiver.LogOnAsync(server, username, password));

            if (states.All(state => state == ConnectionStateCode.Success))
            {
                receiver.RequestManager.AddRequestHandler<Request, Response>((request, completion) =>
                {
                    Console.WriteLine($"Request received from {completion.SourceApplication}: {request.Message}");
                    completion.SetResponse(new Response { Reply = "PONG" });
                });

                Console.WriteLine($"Sending  request to: {receiver.Client.Guid}");
                Response response = await sender.RequestManager.SendRequestAsync<Request, Response>(receiver.Client.Guid, new Request { Message = "PING" });
                Console.WriteLine($"Response received: {response.Reply}");
            }
            else
            {
                Console.WriteLine($"Logon failed: {string.Join(", ", states)}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
