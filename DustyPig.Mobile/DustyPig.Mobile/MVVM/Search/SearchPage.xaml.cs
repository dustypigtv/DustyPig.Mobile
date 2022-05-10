using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Search
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
            BindingContext = VM = new SearchViewModel();           
        }

        public SearchViewModel VM { get; }

        private async void Poster_Tapped(object sender, System.EventArgs e) => await sender.TapEffect();

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (string.IsNullOrWhiteSpace(TheSearchBar.Text))
                {
                    while (!TheSearchBar.Focus()) 
                    {
                        await Task.Delay(100);
                    }
                }
            });
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e) => await VM.DoSearch(e.NewTextValue);

    }
}