// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Sdk;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        private static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            var sender = new Engine();
            var receiver = new Engine(); 
            
            await Task.WhenAll(sender.LogOnAsync(server, username, password), receiver.LogOnAsync(server, username, password));

            receiver.RequestManager.AddRequestHandler<Request, Response>((request, completion) =>
            {
                Console.WriteLine($"Request received from {completion.SourceApplication}: {request.Message}");
                completion.SetResponse(new Response { Reply = "PONG" });
            });

            Console.WriteLine($"Sending  request to: {receiver.Client.Guid}");
            Response response = await sender.RequestManager.SendRequestAsync<Request, Response>(receiver.Client.Guid, new Request { Message = "PING" });
            Console.WriteLine($"Response received: {response.Reply}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
