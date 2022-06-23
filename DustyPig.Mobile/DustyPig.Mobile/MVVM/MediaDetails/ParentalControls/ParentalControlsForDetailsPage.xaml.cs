using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParentalControlsForDetailsPage : ContentPage
    {
        public ParentalControlsForDetailsPage(int id, int libraryId)
        {
            InitializeComponent();

            BindingContext = VM = new ParentalControlsForDetailsVeiwModel(id, libraryId, Navigation);
        }

        public ParentalControlsForDetailsVeiwModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        
    }
}