using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    public class ItemBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Color.White : Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Color.White : Color.Black;
        }
    }
}
