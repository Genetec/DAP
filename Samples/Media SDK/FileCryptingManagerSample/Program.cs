// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media.Export;

SdkResolver.Initialize();

// Get the file path and password from user input
string filePath = ReadFilePath();
string password = ReadPassword();

// Set up cancellation support
using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Cancelling...");
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

// Process the file (encrypt or decrypt based on extension)
await ProcessFile(cancellationTokenSource.Token);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

// Prompts user for a valid media file path
// Accepts .mp4 (for encryption) or .gek (for decryption)
// Also accepts .g64 and .g64x formats
string ReadFilePath()
{
    while (true)
    {
        Console.Write("Enter media file path: ");
        string input = Console.ReadLine();
        if (File.Exists(input))
        {
            string extension = Path.GetExtension(input).ToLower();
            if (extension is ".mp4" or ".g64" or ".g64x" or ".gek")
            {
                return input;
            }
            Console.WriteLine("Invalid file type. Please enter a .mp4, .g64, .g64x, or .gek file.");
        }
        else
        {
            Console.WriteLine("File not found. Please enter a valid file path.");
        }
    }
}

// Prompts user for a valid encryption/decryption password
// Password must be exactly 16, 24, or 32 ASCII characters
// This matches AES key length requirements (128, 192, or 256 bits)
string ReadPassword()
{
    while (true)
    {
        Console.Write("Enter password: ");
        string password = ReadPasswordMasked();
        Console.WriteLine();  // Move to the next line after password input

        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Password cannot be empty.");
            continue;
        }

        if (Encoding.ASCII.GetByteCount(password) is not (16 or 24 or 32))
        {
            Console.WriteLine("Password must be 16, 24, or 32 ASCII characters long.");
            continue;
        }

        return password;
    }

    // Helper function to mask password input with asterisks
    // Supports backspace for correction
    string ReadPasswordMasked()
    {
        StringBuilder builder = new();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            if (key.Key == ConsoleKey.Backspace && builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                builder.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return builder.ToString();
    }
}

// Uses FileCryptingManager from the SDK to encrypt or decrypt the file
// Operation is determined by file extension (.gek files are decrypted, others are encrypted)
async Task ProcessFile(CancellationToken token)
{
    // Determine operation based on file extension
    bool decrypt = Path.GetExtension(filePath).ToLower() == ".gek";
    Console.WriteLine(decrypt ? "File will be decrypted." : "File will be encrypted.");

    // Create a FileCryptingManager instance to handle encryption/decryption
    using var cryptingManager = new FileCryptingManager();

    // Set up progress reporting
    var progress = new Progress<int>(percent => Console.Write($"\rProgress: {percent}%"));

    // Perform encryption or decryption
    FileEncryptionResult result = decrypt ?
        await cryptingManager.DecryptAsync(filePath, password, progress, token) :
        await cryptingManager.EncryptAsync(filePath, password, progress, token);

    Console.WriteLine(); // Move to a new line after progress reporting

    // Report the result
    Console.WriteLine(result.Success
        ? $"{(decrypt ? "Decryption" : "Encryption")} successful. Output file: {result.OutputFile}"
        : $"{(decrypt ? "Decryption" : "Encryption")} failed.");
}