using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Auth.Signup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignupPage : ContentPage
    {
        public SignupPage()
        {
            InitializeComponent();
            BindingContext = VM = new SignupViewModel(Navigation);
        }

        public SignupViewModel VM { get; }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            while (!UserName.Focus())
            {
                await Task.Delay(100);
            }
        }

        private void Name_Completed(object sender, System.EventArgs e)
        {
            Email.Focus();
        }

        private void Email_Completed(object sender, System.EventArgs e)
        {
            Password.Focus();
        }

        private void Password_Completed(object sender, System.EventArgs e)
        {
            Signup.Command.Execute(null);
        }
    }
}