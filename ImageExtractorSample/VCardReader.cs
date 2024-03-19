// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
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
            var vcard = new VCard
            {
                FirstName = ExtractField(vCardText, "FN:"),
                LastName = ExtractField(vCardText, "N:").Split(';')[0], // Assuming last name is the first component in the N: field
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
            Match photoMatch = Regex.Match(vCardText, @"PHOTO;ENCODING=b;TYPE=image/jpeg:(.*?)(\n(?![ \t])|\r\n(?![ \t])|$)", RegexOptions.Singleline);
            if (photoMatch.Success)
            {
                string base64Data = photoMatch.Groups[1].Value.Trim().Replace("\n", "").Replace("\r", "");

                using (var stream = new MemoryStream(Convert.FromBase64String(base64Data)))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }

            return null;
        }
    }
}