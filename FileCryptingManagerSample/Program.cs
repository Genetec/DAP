using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media.Export;

SdkResolver.Initialize();

string filePath = ReadFilePath();
char operation = ReadOperation();
string password = ReadPassword();

await ProcessFile();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

string ReadFilePath()
{
    while (true)
    {
        Console.Write("\nEnter media file path: ");
        string input = Console.ReadLine();

        if (File.Exists(input))
        {
            return input;
        }

        Console.WriteLine("File not found. Please enter a valid file path.");
    }
}

char ReadOperation()
{
    while (true)
    {
        Console.Write("Enter 'e' to encrypt or 'd' to decrypt: ");
        char input = Console.ReadKey().KeyChar;
        Console.WriteLine(); // Move to the next line after operation input

        if (input is not ('e' or 'd'))
        {
            Console.WriteLine("Invalid operation. Please enter 'e' or 'd'.");
            continue;
        }

        return input;
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
    using var cryptingManager = new FileCryptingManager();

    FileEncryptionResult result = operation == 'e' ?
        await cryptingManager.EncryptAsync(filePath, password) : 
        await cryptingManager.DecryptAsync(filePath, password);
    Console.WriteLine(result.Success
        ? $"{(operation == 'e' ? "Encryption" : "Decryption")} successful. Output file: {result.OutputFile}"
        : $"{(operation == 'e' ? "Encryption" : "Decryption")} failed.");
}