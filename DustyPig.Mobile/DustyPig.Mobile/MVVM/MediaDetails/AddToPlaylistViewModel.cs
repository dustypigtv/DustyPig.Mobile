using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public class AddToPlaylistViewModel : _BaseViewModel
    {
        public AddToPlaylistViewModel(INavigation navigation, BasicMedia bm, Action dismiss) : base(navigation)
        {

            CancelCommand = new Command(dismiss);
            NewPlaylistCommand = new AsyncCommand(OnNewPlaylist, canExecute: CanTapNewPlaylist, allowsMultipleExecutions: false);

            
        }

        public Command CancelCommand { get; }


        private bool CanTapNewPlaylist() => !string.IsNullOrWhiteSpace(_newPlaylistText);

        private string _newPlaylistText;
        public string NewPlaylistText
        {
            get => _newPlaylistText;
            set
            {
                SetProperty(ref _newPlaylistText, value);
                NewPlaylistCommand.ChangeCanExecute();
            }
        }



        public AsyncCommand NewPlaylistCommand { get; }
        private async Task OnNewPlaylist()
        {
            
        }


    }
}
