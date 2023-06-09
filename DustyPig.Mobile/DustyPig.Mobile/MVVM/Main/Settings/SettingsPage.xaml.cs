﻿
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage, IPageShown
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public void PageShown(bool reselected)
        {

        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            await Services.LogoutService.LogoutAsync();
        }
    }
}