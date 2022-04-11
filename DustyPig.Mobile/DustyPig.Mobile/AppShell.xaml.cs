using DustyPig.Mobile.CrossPlatform.Orientation;
using DustyPig.Mobile.Views;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            DependencyService.Get<IScreen>().SetOrientation(false);

            Routing.RegisterRoute(nameof(SignupPage), typeof(SignupPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(SelectProfilePage), typeof(SelectProfilePage));
        }

    }
}
