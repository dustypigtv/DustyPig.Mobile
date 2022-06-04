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
            //Services.Settings.DeleteProfileToken();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            App.API.Token = await Services.Settings.GetProfileTokenAsync();

            //For debugging login flow
#if DEBUG
            //string token = App.API.Token;
            //bool breakHere = true;
#endif

            if (string.IsNullOrWhiteSpace(App.API.Token))
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                var response = await App.API.Auth.VerifyTokenAsync();
                if (response.Success && response.Data.LoginType != LoginResponseType.Account)
                {
                    App.IsMainProfile = response.Data.LoginType == LoginResponseType.MainProfile;
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
            }
        }
    }
}