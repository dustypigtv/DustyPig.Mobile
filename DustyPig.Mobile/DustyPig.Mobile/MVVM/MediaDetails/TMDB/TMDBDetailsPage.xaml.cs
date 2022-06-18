﻿using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.TMDB
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TMDBDetailsPage : ContentPage
    {
        public TMDBDetailsPage(BasicTMDB basicTMDB)
        {
            InitializeComponent();
            SCButtons.CloseTapped += (sender, e) => BackgroundColor = Color.Transparent;

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new TMDBDetailsViewModel(basicTMDB, Navigation);
        }

        public TMDBDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Default modal animation is 250 secs
            await Task.Delay(250);
            double alpha = 0;
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(50);
                alpha += 0.1;
                BackgroundColor = Color.FromRgba(0, 0, 0, alpha);
            }
        }
    }
}