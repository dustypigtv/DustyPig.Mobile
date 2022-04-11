using DustyPig.Mobile.CrossPlatform.Orientation;
using DustyPig.Mobile.Views;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            DependencyService.Get<IScreen>().SetOrientation(false);

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));


            CurrentItem = new ShellContent { ContentTemplate = new DataTemplate(typeof(LoginPage)) };
        }

    }
}
