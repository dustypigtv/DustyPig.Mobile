using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddToPlaylistPopup : Popup
    {
        public AddToPlaylistPopup(BasicMedia basicMedia)
        {
            InitializeComponent();

            BindingContext = VM = new AddToPlaylistViewModel(Navigation, basicMedia, () => Dismiss(null));
        }

        public AddToPlaylistViewModel VM { get; }     
    }
}