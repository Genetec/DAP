// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media.Export;

SdkResolver.Initialize();

string filePath = ReadFilePath();
string password = ReadPassword();

await ProcessFile();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

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

async Task ProcessFile()
{
    bool decrypt = Path.GetExtension(filePath).ToLower() == ".gek";
    Console.WriteLine(decrypt ? "File will be decrypted." : "File will be encrypted.");

    using var cryptingManager = new FileCryptingManager();

    var progress = new Progress<int>(percent => Console.Write($"\rProgress: {percent}%"));

    FileEncryptionResult result = decrypt ?
        await cryptingManager.DecryptAsync(filePath, password, progress):
        await cryptingManager.EncryptAsync(filePath, password, progress);

    Console.WriteLine(); // Move to a new line after progress reporting

    Console.WriteLine(result.Success
        ? $"{(decrypt ? "Decryption" : "Encryption")} successful. Output file: {result.OutputFile}"
        : $"{(decrypt ? "Decryption" : "Encryption")} failed.");
}