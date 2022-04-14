using DustyPig.Mobile;
using DustyPig.Mobile.iOS.Renderers;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonRenderer))]
namespace DustyPig.Mobile.iOS.Renderers
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
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
            switch (button.HorizontalTextAlignment)
            {
                case TextAlignment.Center:
                    Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
                    Control.TitleLabel.TextAlignment = UITextAlignment.Center;
                    break;
                case TextAlignment.Start:
                    Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                    Control.TitleLabel.TextAlignment = UITextAlignment.Left;
                    Control.TitleEdgeInsets = new UIEdgeInsets(0, (nfloat)button.Padding.Left, 0, (nfloat)button.Padding.Right);
                    break;
                case TextAlignment.End:
                    Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
                    Control.TitleLabel.TextAlignment = UITextAlignment.Right;
                    Control.TitleEdgeInsets = new UIEdgeInsets(0, (nfloat)button.Padding.Left, 0, (nfloat)button.Padding.Right);
                    break;
                default:
                    Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
                    Control.TitleLabel.TextAlignment = UITextAlignment.Center;
                    break;
            }
        }
    }
}