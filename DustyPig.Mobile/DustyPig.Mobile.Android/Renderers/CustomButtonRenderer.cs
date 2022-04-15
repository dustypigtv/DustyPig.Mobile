using Android.Content;
using Android.Views;
using DustyPig.Mobile;
using DustyPig.Mobile.Controls;
using DustyPig.Mobile.Droid.Renderers;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonRenderer))]
namespace DustyPig.Mobile.Droid.Renderers
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        public CustomButtonRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            SetTextHorizontalAlignment();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == CustomButton.HorizontalTextAlignmentProperty.PropertyName)
                SetTextHorizontalAlignment();
        }

        private void SetTextHorizontalAlignment()
        {
            var button = Element as CustomButton;
            Control.Gravity = button.HorizontalTextAlignment switch
            {
                Xamarin.Forms.TextAlignment.Center => GravityFlags.AxisSpecified | GravityFlags.CenterVertical,
                Xamarin.Forms.TextAlignment.Start => GravityFlags.Left | GravityFlags.CenterVertical,
                Xamarin.Forms.TextAlignment.End => GravityFlags.Right | GravityFlags.CenterVertical,
                _ => GravityFlags.AxisSpecified | GravityFlags.CenterVertical,
            };
        }
    }
}