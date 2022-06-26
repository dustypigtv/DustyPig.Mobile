using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.TMDB
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendPickerPage : ContentPage
    {
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        public FriendPickerPage()
        {
            InitializeComponent();

            BindingContext = VM = new FriendPickerViewModel(_taskCompletionSource, Navigation);
        }

        public FriendPickerViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            VM.CancelCommand.ExecuteAsync();
            return true;
        }

        public Task<int> GetResultAsync() => _taskCompletionSource.Task;
    }
}