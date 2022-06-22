using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaylistEditorPage : ContentPage
    {
        readonly TaskCompletionSource<EditPlaylistResult> _taskCompletionSource = new TaskCompletionSource<EditPlaylistResult>();

        public PlaylistEditorPage(DetailedPlaylist detailedPlaylist)
        {
            InitializeComponent();

            BindingContext = VM = new PlaylistEditorViewModel(detailedPlaylist, _taskCompletionSource, Navigation);
        }

        public PlaylistEditorViewModel VM { get; }

        public Task<EditPlaylistResult> GetResultAsync() => _taskCompletionSource.Task;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

    }
}