using DustyPig.API.v3.Models;
using DustyPig.Mobile.MVVM.Auth.Login;
using DustyPig.Mobile.MVVM.Main;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartupPage : ContentPage
    {

        public StartupPage()
        {
            InitializeComponent();

            //For debugging login flow
            Services.Settings.DeleteProfileToken();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrWhiteSpace(App.API.Token))
                App.API.Token = await Services.Settings.GetProfileTokenAsync();

            //For debugging login flow
#if DEBUG
            string token = App.API.Token;
            bool breakHere = true;
#endif

            if (string.IsNullOrWhiteSpace(App.API.Token))
            {
                //Shell.Current.CurrentItem = new LoginPage();
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                var response = await App.API.Auth.VerifyTokenAsync();
                if (response.Success && response.Data.LoginType == LoginResponseType.Profile)
                    //Shell.Current.CurrentItem = Shell.Current.Items.First(item => item.Route == "Main");
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                else
                    //Shell.Current.CurrentItem = new LoginPage();
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}