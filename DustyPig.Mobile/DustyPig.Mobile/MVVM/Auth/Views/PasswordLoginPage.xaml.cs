using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Auth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordLoginPage : ContentPage
    {
        public PasswordLoginPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            while (!Email.Focus())
            {
                await Task.Delay(100);
            }
        }

        private void Email_Completed(object sender, EventArgs e)
        {
            Password.Focus();
        }

        private void Password_Completed(object sender, EventArgs e)
        {
            Login.Command.Execute(null);
        }
    }
}