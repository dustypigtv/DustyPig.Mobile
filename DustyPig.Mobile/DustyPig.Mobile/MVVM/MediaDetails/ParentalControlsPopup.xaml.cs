using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParentalControlsPopup : Popup
    {
        public ParentalControlsPopup(int id, int libraryId)
        {
            InitializeComponent();
            BindingContext = VM = new ParentalControlsViewModel(Navigation, id, libraryId, () => Dismiss(null));
        }

        public ParentalControlsViewModel VM { get; }

    }
}