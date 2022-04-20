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


        public static readonly BindableProperty ExactXProperty =
        BindableProperty.Create(
            nameof(ExactX),
            typeof(double),
            typeof(MyCollectionView),
            0d);

        public double ExactX
        {
            get => (double)GetValue(ExactXProperty);
            set => SetValue(ExactXProperty, value);
        }
    }
}
