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
        }

        private void Email_Completed(object sender, System.EventArgs e)
        {
            Password.Focus();
        }

        private void Password_Completed(object sender, System.EventArgs e)
        {
            Login.Command.Execute(null);
        }
    }
}