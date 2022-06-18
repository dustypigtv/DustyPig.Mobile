
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Downloads
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadsPage : ContentPage, IPageShown
    {
        public DownloadsPage()
        {
            InitializeComponent();
        }

        public void PageShown(bool reselected)
        {

        }
    }
}