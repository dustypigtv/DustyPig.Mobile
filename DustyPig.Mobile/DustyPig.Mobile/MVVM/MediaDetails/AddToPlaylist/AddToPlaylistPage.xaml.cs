using DustyPig.API.v3.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.AddToPlaylist
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddToPlaylistPage : ContentPage
    {
        public AddToPlaylistPage(BasicMedia basicMedia)
        {
            InitializeComponent();

            BindingContext = VM = new AddToPlaylistViewModel(basicMedia, Navigation);
        }

        public AddToPlaylistViewModel VM { get; }

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
                while (!NewNameEntry.Focus())
                {
                    await Task.Delay(100);
                }
            });

        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}