using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    public class SeasonInfo
    {

        //No idea why I can't bind to a relative source in a popup dialog, so just pass in the desired action and create the command here
        public SeasonInfo(ushort number, string text, Action<int> action)
        {
            Number = number;
            Text = text;

            SeasonTapped = new Command<ushort>(selected => action(selected));
        }

        public ushort Number { get; set; }
        public string Text { get; set; }

        public Command<ushort> SeasonTapped { get; }
    }
}
