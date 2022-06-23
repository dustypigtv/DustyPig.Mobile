﻿using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Explore.Filter
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterPage : ContentPage
    {
        readonly TaskCompletionSource<ExploreRequest> _taskCompletionSource = new TaskCompletionSource<ExploreRequest>();
        readonly ExploreRequest _currentRequest;

        public FilterPage(ExploreRequest currentRequest)
        {
            InitializeComponent();

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            _currentRequest = currentRequest;
            BindingContext = VM = new FilterViewModel(currentRequest, _taskCompletionSource, Navigation);
        }

        public FilterViewModel VM { get; }

        public Task<ExploreRequest> GetResultAsync() => _taskCompletionSource.Task;

        protected override async void OnAppearing()
        {
            base.OnAppearing();


            //Default modal animation is 250 secs
            double alpha = 0;
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(50);
                alpha += 0.1;
                BackgroundColor = Color.FromRgba(0, 0, 0, alpha);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            _taskCompletionSource.TrySetResult(_currentRequest);
            Navigation.PopModalAsync();
            return true;
        }
    }
}