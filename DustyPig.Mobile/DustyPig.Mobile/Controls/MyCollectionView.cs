using Xamarin.Forms;

namespace DustyPig.Mobile.Controls
{
    public class MyCollectionView : CollectionView
    {
        public static readonly BindableProperty MyIdProperty =
        BindableProperty.Create(
            nameof(MyId),
            typeof(long),
            typeof(MyCollectionView),
            0L);

        public long MyId
        {
            get => (long)GetValue(MyIdProperty);
            set => SetValue(MyIdProperty, value);
        }
    }
}