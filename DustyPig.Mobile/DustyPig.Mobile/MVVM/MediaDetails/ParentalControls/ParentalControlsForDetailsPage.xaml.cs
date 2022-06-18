using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ParentalControlsForDetailsPage : Popup
    {
        public ParentalControlsForDetailsPage(int id, int libraryId)
        {
            InitializeComponent();

            //On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new ParentalControlsForDetailsVeiwModel(id, libraryId, Navigation);
        }

        public ParentalControlsForDetailsVeiwModel VM { get; }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);
        //    VM.OnSizeAllocated(width, height);
        //}

       

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PopModalAsync();
        }
    }
}