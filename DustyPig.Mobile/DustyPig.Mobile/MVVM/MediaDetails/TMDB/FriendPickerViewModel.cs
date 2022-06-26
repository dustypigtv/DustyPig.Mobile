using DustyPig.API.v3.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.TMDB
{
    public class FriendPickerViewModel : _DetailsBaseViewModel
    {
        private readonly TaskCompletionSource<int> _taskCompletionSource;

        public FriendPickerViewModel(TaskCompletionSource<int> taskCompletionSource, INavigation navigation) : base(navigation)
        {
            _taskCompletionSource = taskCompletionSource;
            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            FriendTappedCommand = new AsyncCommand<int>(OnFriendTapped, allowsMultipleExecutions: false);

            IsBusy = true;
            LoadData();
        }

        public AsyncCommand CancelCommand { get; }
        private Task OnCancel() => OnFriendTapped(-1);

        public AsyncCommand<int> FriendTappedCommand { get; }
        public async Task OnFriendTapped(int id)
        {
            _taskCompletionSource.TrySetResult(id);
            await Navigation.PopModalAsync();
        }

        public ObservableRangeCollection<BasicFriend> Friends { get; } = new ObservableRangeCollection<BasicFriend>();

        private async void LoadData()
        {
            var response = await App.API.Friends.ListAsync();
            if (response.Success)
            {
                Friends.AddRange(response.Data);
                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
                await CancelCommand.ExecuteAsync();
            }
        }
    }
}
