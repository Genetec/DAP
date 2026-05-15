using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Genetec.Dap.CodeSamples;

public class VCardReader
{
    public static VCard ReadVCard(string filePath)
    {
        string vCardText = File.ReadAllText(filePath);

        // Handle line folding (lines starting with space or tab are continuations)
        vCardText = Regex.Replace(vCardText, @"\r?\n[ \t]", "", RegexOptions.Multiline);

        var vcard = new VCard();

        // Extract name fields properly
        string fullName = ExtractField(vCardText, "FN");
        string structuredName = ExtractField(vCardText, "N");

        vcard.FirstName = ExtractFirstName(fullName, structuredName);
        vcard.LastName = ExtractLastName(structuredName, fullName);
        vcard.Note = ExtractField(vCardText, "NOTE");
        vcard.Picture = ExtractPhoto(vCardText);

        vcard.Emails.AddRange(ExtractEmails(vCardText));
        return vcard;
    }

    private static string ExtractField(string vCardText, string fieldName)
    {
        // More flexible regex that handles various VCard formats
        string pattern = $@"^{Regex.Escape(fieldName)}(?:[^:]*)?:(.*)$";
        Match match = Regex.Match(vCardText, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static string ExtractFirstName(string fullName, string structuredName)
    {
        if (!string.IsNullOrEmpty(structuredName))
        {
            // N: field format is "LastName;FirstName;MiddleName;Prefix;Suffix"
            string[] nameParts = structuredName.Split(';');
            if (nameParts.Length > 1 && !string.IsNullOrEmpty(nameParts[1]))
            {
                return nameParts[1].Trim();
            }
        }

        // Fallback to extracting first word from full name
        if (!string.IsNullOrEmpty(fullName))
        {
            return fullName.Split(' ').FirstOrDefault()?.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string ExtractLastName(string structuredName, string fullName)
    {
        if (!string.IsNullOrEmpty(structuredName))
        {
            // N: field format - last name is the first component
            string[] nameParts = structuredName.Split(';');
            if (nameParts.Length > 0 && !string.IsNullOrEmpty(nameParts[0]))
            {
                return nameParts[0].Trim();
            }
        }

        // Fallback to extracting last word from full name
        if (!string.IsNullOrEmpty(fullName))
        {
            string[] words = fullName.Split(' ');
            return words.Length > 1 ? words.Last().Trim() : string.Empty;
        }

        return string.Empty;
    }

    private static List<string> ExtractEmails(string vCardText)
    {
        var emails = new List<string>();

        // Match EMAIL fields with various parameter formats
        string pattern = @"^EMAIL(?:[^:]*)?:(.+)$";
        MatchCollection matches = Regex.Matches(vCardText, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string email = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(email) && !emails.Contains(email))
            {
                emails.Add(email);
            }
        }

        return emails;
    }

    private static ImageSource ExtractPhoto(string vCardText)
    {
        // Try different photo formats
        var photoPatterns = new[]
        {
            @"PHOTO;ENCODING=b;TYPE=image/jpeg:(.+?)(?=\r?\n[A-Z]|\r?\n$|$)",
            @"PHOTO;ENCODING=BASE64;TYPE=JPEG:(.+?)(?=\r?\n[A-Z]|\r?\n$|$)",
            @"PHOTO;TYPE=JPEG;ENCODING=b:(.+?)(?=\r?\n[A-Z]|\r?\n$|$)",
            @"PHOTO:data:image/jpeg;base64,(.+?)(?=\r?\n[A-Z]|\r?\n$|$)"
        };

        foreach (string pattern in photoPatterns)
        {
            Match photoMatch = Regex.Match(vCardText, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (photoMatch.Success)
            {
                try
                {
                    string base64Data = photoMatch.Groups[1].Value
                        .Replace("\n", "")
                        .Replace("\r", "")
                        .Replace(" ", "")
                        .Replace("\t", "");

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
                catch (Exception)
                {
                    // Continue to next pattern if this one fails
                }
            }
        }

        return null;
    }
}