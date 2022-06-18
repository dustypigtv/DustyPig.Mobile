using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    public class ParentalControlsGroupViewModel : List<ParentalControlsProfileViewModel>
    {
        public ParentalControlsGroupViewModel()
        {
            ShowInfoCommand = new AsyncCommand(OnShowInfo, allowsMultipleExecutions: false);
        }

        public string Header { get; set; }

        public bool ShowIcon { get; set; }

        public string Info { get; set; }

        public AsyncCommand ShowInfoCommand { get; }
        private Task OnShowInfo() => Helpers.Alerts.ShowAlertAsync("Parental Controls Disabled", Info);
    }
}
