using DustyPig.Mobile.Controls;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        //Android destroys views when leaving the tab, and therefore loses the scroll position
        //All this is to restore that position when re-entering this tab
        private static bool IsAndroid => Device.RuntimePlatform == Device.Android;
        private readonly Dictionary<long, int> _sectionScrollPositions = new Dictionary<long, int>();
        private readonly Dictionary<long, bool> _watchScrollPositions = new Dictionary<long, bool>();
        private int _mainScrollPosition = 0;
        private bool _watchMainScrollPosition = IsAndroid;

        public HomePage()
        {
            InitializeComponent();
            BindingContext = VM = new HomeViewModel();
        }


        public HomeViewModel VM { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //Save scroll positions
            if (IsAndroid)
            {
                _watchMainScrollPosition = false;
                foreach (var key in new List<long>(_watchScrollPositions.Keys))
                    _watchScrollPositions[key] = false;
            }
        }

        private void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (!IsAndroid)
                return;

            //Restore scroll positions then start recording again
            var cv = sender as MyCollectionView;
            if (_watchScrollPositions.TryGetValue(cv.MyId, out bool watching))
            {
                if (watching)
                {
                    _sectionScrollPositions[cv.MyId] = e.FirstVisibleItemIndex;
                }
                else if (_sectionScrollPositions.TryGetValue(cv.MyId, out int position) && position > 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        cv.ScrollTo(position, -1, ScrollToPosition.Start, false);
                        _watchScrollPositions[cv.MyId] = true;
                    });
                }
                else
                {
                    _watchScrollPositions[cv.MyId] = true;
                    _sectionScrollPositions[cv.MyId] = e.FirstVisibleItemIndex;
                }
            }
            else
            {
                _watchScrollPositions[cv.MyId] = true;
                _sectionScrollPositions[cv.MyId] = e.FirstVisibleItemIndex;
            }
        }

        private void MainCollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (!IsAndroid)
                return;


            //Restore scroll positions then start recording again
            if (_watchMainScrollPosition)
            {
                _mainScrollPosition = e.FirstVisibleItemIndex;
            }
            else if (_mainScrollPosition > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MainCollectionView.ScrollTo(_mainScrollPosition, -1, ScrollToPosition.Start, false);
                    _watchMainScrollPosition = true;
                });
            }
            else
            {
                _mainScrollPosition = e.FirstVisibleItemIndex;
                _watchMainScrollPosition = true;
            }
        }


    }
}