using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OptionsDialog : Popup<DetailsOptions>
    {
        public OptionsDialog()
        {
            InitializeComponent();

            PlaylistCommand = new Command(() => Dismiss(DetailsOptions.AddToPlaylist));
            ParentalControlsCommand = new Command(() => Dismiss(DetailsOptions.ParentalControls));
            CancelCommand = new Command(() => Dismiss(DetailsOptions.None));

            BindingContext = this;
        }


        public Command PlaylistCommand { get; }

        public Command ParentalControlsCommand { get; }

        public Command CancelCommand { get; }

    }
}