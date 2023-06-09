﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeriesDetailsPage : ContentPage
    {
        public SeriesDetailsPage(BasicMedia basicMedia, StackLayout slDimmer)
        {
            InitializeComponent();

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new SeriesDetailsViewModel(basicMedia, Navigation);

            SCButtons.CloseTapped += (sender, e) => slDimmer?.BrightenSL();
            slDimmer.DimSL();
        }

        public SeriesDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            SCButtons.CloseButtonTapped.ExecuteAsync();
            return true;
        }
    }
}