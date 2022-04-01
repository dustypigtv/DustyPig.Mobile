using DustyPig.Mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}