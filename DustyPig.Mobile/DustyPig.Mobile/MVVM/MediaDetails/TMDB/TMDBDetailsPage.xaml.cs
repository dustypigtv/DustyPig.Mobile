﻿using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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