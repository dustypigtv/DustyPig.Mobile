﻿using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovieDetailsPage : ContentPage
    {
        public MovieDetailsPage(BasicMedia basicMedia)
        {
            InitializeComponent();

            SCButtons.CloseTapped += (sender, e) => BackgroundColor = Color.Transparent;

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);
            
            BindingContext = VM = new MovieDetailsViewModel(basicMedia, Navigation);
        }

        public MovieDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
            Device.BeginInvokeOnMainThread(async () =>
            {
                //Default modal animation is 250 secs
                await Task.Delay(250);
                BackgroundColor = Color.FromRgba(0, 0, 0, 0.5);
            });
        }
    }
}