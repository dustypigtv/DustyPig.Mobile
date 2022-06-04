using DustyPig.Mobile.iOS.Renderers;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchBar), typeof(FixedSearchBarRenderer))]
namespace DustyPig.Mobile.iOS.Renderers
{
    public class FixedSearchBarRenderer : SearchBarRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;

            Control.SearchTextField.LeftView.TintColor = e.NewElement.PlaceholderColor.ToUIColor();
            Control.SearchTextField.BackgroundColor = e.NewElement.BackgroundColor.ToUIColor();
            Control.BackgroundColor = Color.Transparent.ToUIColor();
            Control.SetShowsCancelButton(false, false);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Override needed, otherwise the original Xamarin code will force show the Cancel button
            if (Control != null && e.PropertyName == SearchBar.TextProperty.PropertyName)
                Control.Text = Element.Text;

            if (e.PropertyName != SearchBar.CancelButtonColorProperty.PropertyName && e.PropertyName != SearchBar.TextProperty.PropertyName)
                base.OnElementPropertyChanged(sender, e);
        }
    }
}