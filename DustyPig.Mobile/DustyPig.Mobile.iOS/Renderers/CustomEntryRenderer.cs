using DustyPig.Mobile.iOS.Renderers;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace DustyPig.Mobile.iOS.Renderers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Layer.BorderColor = UIColor.FromRGB(Theme.Placeholder.R, Theme.Placeholder.G, Theme.Placeholder.B).CGColor;
                Control.Layer.BorderWidth = 1;
                //Control.BorderStyle = UITextBorderStyle.None;
            }
        }
    }
}