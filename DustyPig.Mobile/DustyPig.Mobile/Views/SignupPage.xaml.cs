using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignupPage : ContentPage
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            while (!UserName.Focus())
            {
                await Task.Delay(100);
            }
        }

        private void Name_Completed(object sender, System.EventArgs e)
        {
            Email.Focus();
        }

        private void Email_Completed(object sender, System.EventArgs e)
        {
            Password.Focus();
        }

        private void Password_Completed(object sender, System.EventArgs e)
        {
            Signup.Command.Execute(null);
        }
    }
}