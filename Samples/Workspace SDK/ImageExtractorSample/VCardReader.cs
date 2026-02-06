// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class VCardReader
{
    public static VCard ReadVCard(string filePath)
    {
        string vCardText = File.ReadAllText(filePath);

        // N: field format is LastName;FirstName;MiddleName;Prefix;Suffix
        string nameField = ExtractField(vCardText, "N:");
        string[] nameParts = string.IsNullOrEmpty(nameField)
            ? Array.Empty<string>()
            : nameField.Split(';');

        var vcard = new VCard
        {
            LastName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
            FirstName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            Note = ExtractField(vCardText, "NOTE:"),
            Picture = ExtractPhoto(vCardText)
        };

        vcard.Emails.AddRange(ExtractEmails(vCardText));
        return vcard;
    }

    private static string ExtractField(string vCardText, string fieldName)
    {
        Match match = Regex.Match(vCardText, $"{fieldName}(.*?)(\n(?![ \t])|\r\n(?![ \t])|$)", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static List<string> ExtractEmails(string vCardText)
    {
        MatchCollection matches = Regex.Matches(vCardText, "EMAIL;[^:]+:(.*?)\r?\n");
        return matches.Cast<Match>().Select(match => match.Groups[1].Value.Trim()).ToList();
    }

    private static ImageSource ExtractPhoto(string vCardText)
    {
        // Match various vCard photo formats:
        // PHOTO;ENCODING=b;TYPE=image/jpeg:
        // PHOTO;ENCODING=BASE64;TYPE=JPEG:
        // PHOTO;TYPE=JPEG;ENCODING=b:
        // PHOTO;ENCODING=b;TYPE=image/png:
        var photoMatch = Regex.Match(vCardText,
            @"PHOTO;[^:]*(?:ENCODING=(?:b|BASE64))[^:]*:(.*?)(\n(?![ \t])|\r\n(?![ \t])|$)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (photoMatch.Success)
        {
            string base64Data = photoMatch.Groups[1].Value
                .Trim()
                .Replace("\n", "")
                .Replace("\r", "");

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            using var stream = new MemoryStream(imageBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        return null;
    }
}