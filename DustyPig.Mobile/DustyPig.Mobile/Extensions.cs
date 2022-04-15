using System;

namespace DustyPig.Mobile
{
    public static class Extensions
    {
        public static string FormatMessage(this Exception ex) => ex.Message.Trim(new char[] { ' ', '"' });
    }
}
