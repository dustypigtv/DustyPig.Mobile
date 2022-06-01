// https://criticalhittech.com/2017/09/14/tab-reselection-in-xamarin-forms-part-1/

using DustyPig.Mobile.iOS.Renderers;
using DustyPig.Mobile.MVVM.Main;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MainPage), typeof(CustomTabbedPageRenderer))]
namespace DustyPig.Mobile.iOS.Renderers
{
    public class CustomTabbedPageRenderer : TabbedRenderer
    {
        private UIKit.UITabBarItem _prevItem;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (SelectedIndex < TabBar.Items.Length)
                _prevItem = TabBar.Items[SelectedIndex];
        }

        public override void ItemSelected(UIKit.UITabBar tabbar, UIKit.UITabBarItem item)
        {
            if (_prevItem == item && Element is MainPage mp)
                mp.OnTabReselected();
            
            _prevItem = item;
        }
    }
}