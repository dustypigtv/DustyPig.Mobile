using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.LoginButtons
{
    public class XamFacebook : View
    {
        public static readonly BindableProperty OnSuccessProperty = BindableProperty.Create(nameof(OnSuccess), typeof(Command<string>), typeof(XamFacebook));

        public Command<string> OnSuccess
        {
            get => (Command<string>)GetValue(OnSuccessProperty);
            set => SetValue(OnSuccessProperty, value);
        }



        public static readonly BindableProperty OnErrorProperty = BindableProperty.Create(nameof(OnError), typeof(Command<string>), typeof(XamFacebook));

        public Command<string> OnError
        {
            get => (Command<string>)GetValue(OnErrorProperty);
            set => SetValue(OnErrorProperty, value);
        }



        public static readonly BindableProperty OnCancelProperty = BindableProperty.Create(nameof(OnCancel), typeof(Command), typeof(XamFacebook));
        
        public Command OnCancel
        {
            get => (Command)GetValue(OnCancelProperty);
            set => SetValue(OnCancelProperty, value);
        }



        public static readonly BindableProperty PaddingTopProperty = BindableProperty.Create(nameof(PaddingTop), typeof(float), typeof(XamFacebook), 5.091f, BindingMode.OneWay);

        public float PaddingTop
        {
            get => (float)GetValue(PaddingTopProperty);
            set => SetValue(PaddingTopProperty, value);
        }



        public static readonly BindableProperty PaddingBottomProperty = BindableProperty.Create(nameof(PaddingBottom), typeof(float), typeof(XamFacebook), 5.091f, BindingMode.OneWay);

        public float PaddingBottom
        {
            get => (float)GetValue(PaddingBottomProperty);
            set => SetValue(PaddingBottomProperty, value);
        }



        public static readonly BindableProperty TextSizeProperty = BindableProperty.Create(nameof(TextSize), typeof(float), typeof(XamFacebook), 14.02f, BindingMode.OneWay);

        public float TextSize
        {
            get => (float)GetValue(TextSizeProperty);
            set => SetValue(TextSizeProperty, value);
        }
    }
}
