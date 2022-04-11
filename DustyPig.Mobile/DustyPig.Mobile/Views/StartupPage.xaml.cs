using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartupPage : ContentPage
    {
        public StartupPage()
        {
            InitializeComponent();

            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrWhiteSpace(App.API.Token))
                Shell.Current.CurrentItem = new LoginPage();
            else
                Shell.Current.CurrentItem = Shell.Current.Items.First(item => item.Route == "Main");
        }
    }
}