using Android.Content;
using Android.Views;
using DustyPig.Mobile.Droid.Renderers;
using DustyPig.Mobile.MVVM.Main;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using static Android.Views.View;

[assembly: ExportRenderer(typeof(MainPage), typeof(CustomTabbedPageRenderer))]
namespace DustyPig.Mobile.Droid.Renderers
{
    public class CustomTabbedPageRenderer : TabbedPageRenderer, BottomNavigationView.IOnNavigationItemReselectedListener
    {
        public CustomTabbedPageRenderer(Context context) : base(context)
        {
            
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && e.NewElement != null)
            {
                for (int i = 0; i <= this.ViewGroup.ChildCount - 1; i++)
                {
                    var childView = this.ViewGroup.GetChildAt(i);
                    if (childView is ViewGroup viewGroup)
                    {
                        for (int j = 0; j <= viewGroup.ChildCount - 1; j++)
                        {
                            var childRelativeLayoutView = viewGroup.GetChildAt(j);
                            if (childRelativeLayoutView is BottomNavigationView bottomNavigationView)
                            {
                                bottomNavigationView.SetOnNavigationItemReselectedListener(this);
                            }
                        }
                    }
                }
            }
        }

        public void OnNavigationItemReselected(IMenuItem p0)
        {
            if (Element is MainPage mp)
                mp.OnTabReselected();
        }
    }
}