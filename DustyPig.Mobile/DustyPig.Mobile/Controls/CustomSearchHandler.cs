using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.Controls
{
    internal class CustomSearchHandler : SearchHandler
    {
        public event EventHandler<string> DoQuery;

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);
            DoQuery?.Invoke(this, newValue);
        }
    }
}
