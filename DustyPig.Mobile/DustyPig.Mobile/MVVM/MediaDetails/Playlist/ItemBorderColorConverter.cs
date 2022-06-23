using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    public class ItemBorderColorConverter : IValueConverter
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
