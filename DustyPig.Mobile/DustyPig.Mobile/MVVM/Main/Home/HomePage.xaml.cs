using DustyPig.Mobile.Controls;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = VM = new HomeViewModel(MainStack, EmptyLabel, Navigation);
        }


        public HomeViewModel VM { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }
    }
}