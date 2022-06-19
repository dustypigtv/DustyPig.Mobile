using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeasonsPopup : Popup<int>
    {
        public SeasonsPopup(List<ushort> seasons, ushort current)
        {
            InitializeComponent();

            double maxHeight = 100 + (seasons.Count * 42);
            if (Device.Idiom == TargetIdiom.Phone)
                maxHeight = Math.Min(Helpers.Screen.Height * 0.75, maxHeight);
            else
                maxHeight = Math.Min(Math.Min(Helpers.Screen.Height, Helpers.Screen.Width) * 0.75, maxHeight);

            Size = new Size(200, maxHeight);

            seasons.Sort();

            //No idea why I can't bind to a relative source in a popup dialog, so just pass in the desired action
            foreach (var season in seasons)
                Seasons.Add(new SeasonInfo(season, $"Season {season}", Dismiss));

            //All seasons are >= 0, so use -1 for cancel
            CancelCommand = new Command(() => Dismiss(-1));

            BindingContext = this;


            Device.BeginInvokeOnMainThread(async () =>
            {
                while (true)
                {
                    try
                    {
                        TheCV.ScrollTo(current, position: ScrollToPosition.Start, animate: false);
                        return;
                    }
                    catch { }
                    await Task.Delay(100);
                }
            });
        }

        public ObservableCollection<SeasonInfo> Seasons { get; } = new ObservableCollection<SeasonInfo>();

        public Command CancelCommand { get; }
    }


}