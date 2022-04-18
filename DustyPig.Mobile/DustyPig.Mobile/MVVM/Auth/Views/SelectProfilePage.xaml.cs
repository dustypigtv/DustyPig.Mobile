using DustyPig.API.v3.Models;
using DustyPig.Mobile.MVVM.Auth.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Auth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectProfilePage : ContentPage
    {
        public SelectProfilePage()
        {
            InitializeComponent();
            BindingContext = VM = new SelectProfileViewModel();
        }

        public SelectProfileViewModel VM { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            VM.OnItemTapped(e.Item as BasicProfile);
        }
    }
}