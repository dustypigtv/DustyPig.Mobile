using DustyPig.API.v3;
using System;

namespace DustyPig.Mobile
{
    public static class Extensions
    {
        public static string FormatMessage(this Exception ex)
        {
            if (ex is ModelValidationException mve)
                return mve.ToString().Trim(new char[] { ' ', '"' });

            return ex.Message.Trim(new char[] { ' ', '"' });
        }
    }
}
