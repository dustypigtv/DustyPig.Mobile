using DustyPig.API.v3;
using DustyPig.API.v3.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

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

        public static async void DimSL(this StackLayout slDimmer, bool force = false)
        {
            if (slDimmer == null)
                return;

            if (!force)
                if (Device.Idiom == TargetIdiom.Phone)
                    return;

            double alpha = 0;
            while (alpha < 0.5)
            {
                alpha += 0.1;
                slDimmer.BackgroundColor = Color.FromRgba(0, 0, 0, alpha);
                await Task.Delay(50);
            }
        }

        public static async void BrightenSL(this StackLayout slDimmer, bool force = false)
        {
            if (slDimmer == null)
                return;

            if (!force)
                if (Device.Idiom == TargetIdiom.Phone)
                    return;

            double alpha = 0.5;
            while (alpha > 0)
            {
                alpha -= 0.1;
                slDimmer.BackgroundColor = Color.FromRgba(0, 0, 0, alpha);
                await Task.Delay(50);
            }
        }

        public static async Task<bool> HandleUnauthorizedException(this Exception ex)
        {
            if (ex == null)
                return false;

            if ((ex.Message + string.Empty).Contains("401 (Unauthorized)"))
            {
                await Services.LogoutService.LogoutAsync();
                return true;
            }

            if (ex.InnerException != null)
                return await ex.InnerException.HandleUnauthorizedException();

            return false;
        }
    }
}
