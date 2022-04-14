/*
    The image-over-button setup I used for the social login buttons has a problem on Android: Fast Renderers!
    It cause the button to redraw over the image.  So use this custom renderer that for those buttons,
    so that I don't have to disable fast rendering throughout the rest of the app
*/

using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public class CustomButton : Button
    {

        public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(CustomButton),
            TextAlignment.Center);

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }
    }
}
