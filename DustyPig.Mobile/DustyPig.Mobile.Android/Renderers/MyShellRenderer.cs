// https://stackoverflow.com/questions/57442717/is-there-a-way-to-center-the-page-title-on-android-when-using-xamarin-forms-shel

using Android.Content;
using Android.Widget;
using DustyPig.Mobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Shell), typeof(MyShellRenderer))]
namespace DustyPig.Mobile.Droid.Renderers
{
    public class MyShellRenderer : ShellRenderer
    {
        public MyShellRenderer(Context context) : base(context)
        {
        }

        protected override IShellToolbarTracker CreateTrackerForToolbar(AndroidX.AppCompat.Widget.Toolbar toolbar)
        {
            return new MyShellToolbarTracker(this, toolbar, ((IShellContext)this).CurrentDrawerLayout);
        }
    }

    public class MyShellToolbarTracker : ShellToolbarTracker
    {
        public MyShellToolbarTracker(IShellContext shellContext, AndroidX.AppCompat.Widget.Toolbar toolbar, AndroidX.DrawerLayout.Widget.DrawerLayout drawerLayout)
            : base(shellContext, toolbar, drawerLayout)
        {
        }

        protected override void UpdateTitleView(Context context, AndroidX.AppCompat.Widget.Toolbar toolbar, View titleView)
        {
            base.UpdateTitleView(context, toolbar, titleView);

            if (string.IsNullOrEmpty(toolbar.Title))
                return;

            for (int index = 0; index < toolbar.ChildCount; index++)
            {
                if (toolbar.GetChildAt(index) is TextView title)
                {
                    title.LayoutParameters.Width = Android.Views.ViewGroup.LayoutParams.MatchParent;
                    title.TextAlignment = Android.Views.TextAlignment.Center;
                    title.SetPadding(title.PaddingLeft - toolbar.CurrentContentInsetLeft, title.PaddingTop, title.PaddingRight, title.PaddingBottom);
                }
            }
        }
    }
}