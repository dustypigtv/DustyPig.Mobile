using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeasonsDialog : Popup<int>
    {
        public SeasonsDialog(List<ushort> seasons)
        {
            InitializeComponent();
                     

            seasons.Sort();

            //No idea why I can't bind to a relative source in a popup dialog, so just pass in the desired action
            foreach (var season in seasons)
                Seasons.Add(new SeasonInfo(season, $"Season {season}", Dismiss));
                      
            //All seasons are >= 0, so use -1 for cancel
            CancelCommand = new Command(() => Dismiss(-1));

            BindingContext = this;
        }

        public ObservableCollection<SeasonInfo> Seasons { get; } = new ObservableCollection<SeasonInfo>();

        public Command CancelCommand { get; }
    }

    
}