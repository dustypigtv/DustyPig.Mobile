using DustyPig.API.v3;
using System;
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

        public static async Task TapEffect(this object view, double size = 0.95, uint milliseconds = 75)
        {
            var v = view as View;
            if (v == null)
                return;
            await v.ScaleTo(size, milliseconds);
            await v.ScaleTo(1, 75);
        }
    }
}
