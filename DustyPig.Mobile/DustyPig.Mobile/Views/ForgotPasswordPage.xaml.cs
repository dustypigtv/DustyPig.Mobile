using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            while(!Email.Focus())
            {
                await Task.Delay(100);
            }
        }

        private void Email_Completed(object sender, System.EventArgs e)
        {
            Submit.Command.Execute(null);
        }
    }
}