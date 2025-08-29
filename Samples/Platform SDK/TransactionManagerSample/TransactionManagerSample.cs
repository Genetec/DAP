// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

public class TransactionManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
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

    private void CreateTransaction(Engine engine, Action action)
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

    private async Task ExecuteTransactionAsync(Engine engine, Action action)
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