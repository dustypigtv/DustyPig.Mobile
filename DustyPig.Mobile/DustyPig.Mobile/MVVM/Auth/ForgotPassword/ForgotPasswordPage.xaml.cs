using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Auth.ForgotPassword
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
            BindingContext = VM = new ForgotPasswordViewModel(Navigation);
        }

        public ForgotPasswordViewModel VM { get; }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            while (!Email.Focus())
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