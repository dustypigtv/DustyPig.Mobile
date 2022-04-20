﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.MVVM.Auth.Views;
using DustyPig.Mobile.Services;
using System.Linq;
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
            //Settings.DeleteProfileToken();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrWhiteSpace(App.API.Token))
                App.API.Token = await Services.Settings.GetProfileTokenAsync();

            if (string.IsNullOrWhiteSpace(App.API.Token))
            {
                Shell.Current.CurrentItem = new LoginPage();
            }
            else
            {
                var response = await App.API.Auth.VerifyTokenAsync();
                if (response.Success && response.Data.LoginType == LoginResponseType.Profile)
                    Shell.Current.CurrentItem = Shell.Current.Items.First(item => item.Route == "Main");
                else
                    Shell.Current.CurrentItem = new LoginPage();
            }
        }
    }
}