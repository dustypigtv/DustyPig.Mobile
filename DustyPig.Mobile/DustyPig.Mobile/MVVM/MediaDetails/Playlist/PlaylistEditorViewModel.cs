using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using System.Linq;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    public class PlaylistEditorViewModel : _DetailsBaseViewModel
    {
        readonly TaskCompletionSource<EditPlaylistResult> _taskCompletionSource;
        
        public PlaylistEditorViewModel(DetailedPlaylist detailedPlaylist, TaskCompletionSource<EditPlaylistResult> taskCompletionSource, INavigation navigation) : base(navigation)
        {
            Detailed_Playlist = detailedPlaylist;
            _taskCompletionSource = taskCompletionSource;

            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);
            DeleteCommand = new AsyncCommand<int>(OnDelete, allowsMultipleExecutions: false);

            ResetItems();
        }

        private void ResetItems()
        {
            Items.Clear();
            Items.AddRange(Detailed_Playlist.Items);
        }

        public ObservableRangeCollection<PlaylistItem> Items { get; } = new ObservableRangeCollection<PlaylistItem>();

        public AsyncCommand CancelCommand { get; }
        private async Task OnCancel()
        {
            _taskCompletionSource.SetResult(EditPlaylistResult.NoChange);
            await Navigation.PopModalAsync();
        }

        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            bool changed = Items.Count != Detailed_Playlist.Items.Count;
            if(!changed)
                for(int i = 0; i < Items.Count; i++)
                    if (Items[i].Id != Detailed_Playlist.Items[i].Id)
                    {
                        changed = true;
                        break;
                    }

            if (!changed)
            {
                _taskCompletionSource.SetResult(EditPlaylistResult.NoChange);
                await Navigation.PopModalAsync();
                return;
            }

            if(Items.Count == 0)
            {
                var response = await App.API.Playlists.DeleteAsync(Detailed_Playlist.Id);
                if (response.Success)
                {
                    Services.Download.DownloadService.Delete(Detailed_Playlist.Id);
                    _taskCompletionSource.SetResult(EditPlaylistResult.Deleted);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await ShowAlertAsync("Error", response.Error.Message);
                    ResetItems();
                }
            }
            else
            {
                var data = new UpdatePlaylistItemsData { Id = Detailed_Playlist.Id };
                foreach (var item in Items)
                    data.MediaIds.Add(item.MediaId);

                var response = await App.API.Playlists.UpdatePlaylistItems(data);
                if (response.Success)
                {
                    var status = await Services.Download.DownloadService.GetStatusAsync(Detailed_Playlist.Id);
                    if (status.Status != Services.Download.JobStatus.NotDownloaded)
                        Services.Download.DownloadService.AddOrUpdatePlaylist(Detailed_Playlist, status.ItemCount);

                    _taskCompletionSource.SetResult(EditPlaylistResult.Updated);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await ShowAlertAsync("Error", response.Error.Message);
                    ResetItems();
                }
            }
        }

        public AsyncCommand<int> DeleteCommand { get; }
        private async Task OnDelete(int id)
        {
            Items.Remove(Items.FirstOrDefault(item => item.Id == id));
            
            //Too easy to remove items too quickly, so small delay
            await Task.Delay(100);
        }

    }
}
