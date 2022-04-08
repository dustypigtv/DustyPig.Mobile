using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DustyPig.Mobile.LoginButtons
{
    public class XamFacebook : View
    {
        public Command<string> OnSuccess
        {
            get { return (Command<string>)GetValue(OnSuccessProperty); }
            set { SetValue(OnSuccessProperty, value); }
        }

        public static readonly BindableProperty OnSuccessProperty =
            BindableProperty.Create(nameof(OnSuccess), typeof(Command<string>), typeof(XamFacebook));

        public Command<string> OnError
        {
            get { return (Command<string>)GetValue(OnErrorProperty); }
            set { SetValue(OnErrorProperty, value); }
        }

        public static readonly BindableProperty OnErrorProperty =
            BindableProperty.Create(nameof(OnError), typeof(Command<string>), typeof(XamFacebook));

        public Command OnCancel
        {
            get { return (Command)GetValue(OnCancelProperty); }
            set { SetValue(OnCancelProperty, value); }
        }

        public static readonly BindableProperty OnCancelProperty =
            BindableProperty.Create(nameof(OnCancel), typeof(Command), typeof(XamFacebook));
    }
}
