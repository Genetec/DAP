// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);
           
            if (state == ConnectionStateCode.Success)
            {
                // Define an action to create a cardholder
                Action action = () =>
                {
                    Console.WriteLine("Creating a cardholder inside of a transaction...");

                    // Create a new cardholder entity
                    var cardholder = (Cardholder)engine.CreateEntity("John Doe", EntityType.Cardholder);
                    cardholder.FirstName = "John";
                    cardholder.LastName = "Doe";
                    cardholder.EmailAddress = "johndoe@example.com";

                    Console.WriteLine("Cardholder created successfully.");
                };

                // Execute the action within a transaction using two different methods
                Console.WriteLine("Executing transaction using CreateTransaction:");
                CreateTransaction(engine, action);

                Console.WriteLine("Executing transaction using ExecuteTransactionAsync:");
                await ExecuteTransactionAsync(engine, action);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void CreateTransaction(Engine engine, Action action)
        {
            Console.WriteLine("Starting transaction...");
            Console.WriteLine($"IsTransactionActive (before): {engine.TransactionManager.IsTransactionActive}");

            // Begin a new transaction
            engine.TransactionManager.CreateTransaction();
            try
            {
                // Perform the action within the transaction
                action();

                // Commit the transaction if successful
                engine.TransactionManager.CommitTransaction(rollBackOnFailure: true);
                Console.WriteLine("Transaction committed successfully.");
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of failure
                Console.WriteLine($"Transaction failed: {ex.Message}");
                engine.TransactionManager.RollbackTransaction();
                Console.WriteLine("Transaction rolled back.");
            }

            Console.WriteLine($"IsTransactionActive (after): {engine.TransactionManager.IsTransactionActive}");
        }

        static async Task ExecuteTransactionAsync(Engine engine, Action action)
        {
            Console.WriteLine("Starting transaction...");
            Console.WriteLine($"IsTransactionActive (before): {engine.TransactionManager.IsTransactionActive}");

            try
            {
                // Perform the action within the transaction asynchronously
                await engine.TransactionManager.ExecuteTransactionAsync(action);
                Console.WriteLine("Transaction executed successfully.");
            }
            catch (SdkException ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
            }

            Console.WriteLine($"IsTransactionActive (after): {engine.TransactionManager.IsTransactionActive}");
        }
    }
}