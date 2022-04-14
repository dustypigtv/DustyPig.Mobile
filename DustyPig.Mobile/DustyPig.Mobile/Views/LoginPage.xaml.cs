using DustyPig.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            BindingContext = VM = new LoginViewModel();
        }

        public LoginViewModel VM { get; }

        private void AppleSignInButton_SignIn(object sender, System.EventArgs e)
        {

        }
    }
}