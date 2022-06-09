using DustyPig.API.v3;
using DustyPig.API.v3.Models;
using System;
using System.IO;

namespace DustyPig.Mobile.Helpers
{
    public static class Extensions
    {
        public static string FormatMessage(this Exception ex)
        {
            if (ex == null)
                return string.Empty;

            if (ex is ModelValidationException mve)
                return mve.ToString().Trim(new char[] { ' ', '"' });

            string message = (ex.Message + string.Empty).Trim();
            if (message.StartsWith("\"") && message.EndsWith("\""))
                message = message.Substring(1, message.Length - 2);

            return message.Trim();
        }

        public static string SafeFilename(this ExternalSubtitle sub)
        {
            string ret = sub.Name;
            foreach (char c in Path.GetInvalidFileNameChars())
                ret = ret.Replace(c, '_');
            return ret + ".srt";
        }
    }
}
