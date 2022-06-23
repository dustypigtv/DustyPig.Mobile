using System;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    public class EpisodeBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Color.White : Color.FromUint(Helpers.Theme.DarkDarkGrey.ToUInt());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Color.White : Color.FromUint(Helpers.Theme.DarkDarkGrey.ToUInt());
        }
    }
}
