using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.Controls
{
    public class HomePageSection : StackLayout
    {
        private const int FONT_SIZE = 16;
        private const int POSTER_HEIGHT = 150;
        private const int ITEM_SPACING = 16;
        private const string POSTER_ERROR = "resource://DustyPig.Mobile.Images.poster_error.png";
        private const string POSTER_PLACEHOLDER = "resource://DustyPig.Mobile.Images.poster_placeholder.png";

        private static readonly LinearItemsLayout CVLayout = (LinearItemsLayout)LinearItemsLayout.Horizontal;

        private readonly Label _label;
        private readonly CollectionView _collectionView;

        static HomePageSection()
        {
            CVLayout.ItemSpacing = ITEM_SPACING;
        }

        public HomePageSection(AsyncCommand<BasicMedia> itemTappedCommand)
        {
            Orientation = StackOrientation.Vertical;

            _label = new Label 
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = FONT_SIZE
            };
            Children.Add(_label);


            _collectionView = new CollectionView
            {
                ItemsLayout = CVLayout,
                HeightRequest = POSTER_HEIGHT,

                ItemTemplate = new DataTemplate(() =>
                {
                    FFImageLoading.Forms.CachedImage img = new FFImageLoading.Forms.CachedImage
                    {
                        BackgroundColor = Color.Black,
                        ErrorPlaceholder = POSTER_ERROR,
                        LoadingPlaceholder = POSTER_PLACEHOLDER
                    };
                    img.SetBinding(FFImageLoading.Forms.CachedImage.SourceProperty, nameof(BasicMedia.ArtworkUrl));
                    
                    var tap = new TapGestureRecognizer();
                    tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");

                    tap.Tapped += async (sender, e) =>
                    {
                        await img.ScaleTo(0.95, 75);
                        await img.ScaleTo(1, 75);
                        await itemTappedCommand?.ExecuteAsync(((TappedEventArgs)e).Parameter as BasicMedia);
                    };
                    img.GestureRecognizers.Add(tap);

                    return img;
                })
            };

            _collectionView.BindingContext = this;
            _collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(MediaItems));
            Children.Add(_collectionView);
        }

        public long ListId { get; set; }

        public string Title
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        private RangeObservableCollection<BasicMedia> _mediaItems = new RangeObservableCollection<BasicMedia>();
        public RangeObservableCollection<BasicMedia> MediaItems
        {
            get => _mediaItems;
            set
            {
                _mediaItems = value;
                OnPropertyChanged(nameof(MediaItems));
            }
        }


    }
}
