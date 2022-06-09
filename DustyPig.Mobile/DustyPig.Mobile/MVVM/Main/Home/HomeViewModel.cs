using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class HomeViewModel : _BaseViewModel
    {
        private static event EventHandler<BasicMedia> AddedToWatchlist;
        private static event EventHandler<BasicMedia> RemovedFromWatchlist;

        private static event EventHandler<int> MarkWatched;

        private static event EventHandler<BasicMedia> AddedToPlaylists;

        private static event EventHandler<BasicMedia> PlaylistArtworkUpdated;

        public HomeViewModel(StackLayout mainStack, Label emptyLabel, INavigation navigation) : base(navigation)
        {
            MainStack = mainStack;
            EmptyLabel = emptyLabel;

            RefreshCommand = new AsyncCommand(Update);

            AddedToWatchlist += ItemAddedToWatchlist;
            RemovedFromWatchlist += ItemRemovedFromWatchlist;
            MarkWatched += HomeViewModel_MarkWatched;
            AddedToPlaylists += HomeViewModel_AddedToPlaylists;
            PlaylistArtworkUpdated += HomeViewModel_PlaylistArtworkUpdated;

            ////Only do this in the home tab - since this class doesn't get destroyed
            //InternetConnectivityChanged += (sender, e) =>
            //{
            //    var tabBar = Shell.Current.CurrentItem as TabBar;
            //    for (int i = 0; i < 3; i++)
            //        tabBar.Items[i].IsEnabled = e;

            //    if (!e && new string[] { "Home", "Movies", "TV" }.Contains(tabBar.CurrentItem.Title))
            //        tabBar.CurrentItem = tabBar.Items[3];
            //};
        }

        private void HomeViewModel_PlaylistArtworkUpdated(object sender, BasicMedia e)
        {
            try
            {
                var items = GetOrAddSection(DustyPig.API.v3.Clients.MediaClient.ID_PLAYLISTS, DustyPig.API.v3.Clients.MediaClient.ID_PLAYLISTS_TITLE).VM.Items;
                int idx = -1;
                for(int i = 0; i < items.Count; i++)
                    if (items[i].Id == e.Id)
                    {
                        idx = i;
                        break;
                    }
                if (idx < 0)
                    return;
                items.RemoveAt(idx);
                items.Insert(idx, e);
            }
            catch { }
        }

        public static void InvokePlaylistArtworkUpdated(BasicMedia e) => PlaylistArtworkUpdated?.Invoke(null, e);


        private HomePageSectionView GetSection(long id)
        {
            return MainStack.Children.FirstOrDefault(item => ((HomePageSectionView)item).VM.ListId == id) as HomePageSectionView;
        }

        private HomePageSectionView GetOrAddSection(long id, string title)
        {
            var section = GetSection(id);
            if (section == null)
            {
                section = new HomePageSectionView(new HomeScreenList
                {
                    ListId = id,
                    Title = title,
                    Items = new List<BasicMedia>()
                });

                var ids = new List<long>();
                ids.Add(id);
                foreach (HomePageSectionView v in MainStack.Children)
                    ids.Add(v.VM.ListId);
                ids.Sort();
                int idx = ids.IndexOf(id);
                MainStack.Children.Insert(idx, section);
            }

            return section;
        }


        private void HomeViewModel_AddedToPlaylists(object sender, BasicMedia e)
        {
            try
            {
                var section = GetOrAddSection(API.v3.Clients.MediaClient.ID_PLAYLISTS, API.v3.Clients.MediaClient.ID_PLAYLISTS_TITLE);
                section.VM.Items.Add(e);
            }
            catch { }
        }

        public static void InvokeAddedToPlaylists(BasicMedia basicMedia) => AddedToPlaylists?.Invoke(null, basicMedia);


        private void HomeViewModel_MarkWatched(object sender, int e)
        {
            try
            {
                var section = GetSection(API.v3.Clients.MediaClient.ID_CONTINUE_WATCHING);
                if (section == null)
                    return;
                section.VM.Items.Remove(section.VM.Items.First(item => item.Id == e));
                if (section.VM.Items.Count == 0)
                    MainStack.Children.Remove(section);
            }
            catch { }
        }

        public static void InvokeMarkWatched(int id) => MarkWatched?.Invoke(null, id);

        private void ItemAddedToWatchlist(object sender, BasicMedia e)
        {
            try
            {
                var section = GetOrAddSection(API.v3.Clients.MediaClient.ID_WATCHLIST, API.v3.Clients.MediaClient.ID_WATCHLIST_TITLE);
                section.VM.Items.Add(e);
            }
            catch { }
        }

        public static void InvokeAddedToWatchlist(BasicMedia basicMedia) => AddedToWatchlist?.Invoke(null, basicMedia);

        private void ItemRemovedFromWatchlist(object sender, BasicMedia e)
        {
            try
            {
                var section = GetSection(API.v3.Clients.MediaClient.ID_WATCHLIST);
                if (section == null)
                    return;
                section.VM.Items.Remove(e);
                if (section.VM.Items.Count == 0)
                    MainStack.Children.Remove(section);
            }
            catch { }
        }

        public static void InvokeRemovedFromWatchlist(BasicMedia basicMedia) => RemovedFromWatchlist?.Invoke(null, basicMedia);




        

        public AsyncCommand RefreshCommand { get; }

        public StackLayout MainStack { get; }

        public Label EmptyLabel { get; }

        private string _emptyView = "Loading";
        public string EmptyView
        {
            get => _emptyView;
            set => SetProperty(ref _emptyView, value);
        }

        public void OnAppearing()
        {
            if (App.HomePageNeedsRefresh)
                IsBusy = true;
        }
       
        private async Task Update()
        {
            bool loadFromDisk = MainStack.Children.Count == 0;
            if(!loadFromDisk)
                if (MainStack.Children.Count == 1)
                    if (MainStack.Children[0] is Label)
                        loadFromDisk = true;

            if (loadFromDisk)
                try { AddSections(Services.HomePageCache.Load()); }
                catch { }
            
            var response = await App.API.Media.GetHomeScreenAsync();
            if (response.Success)
            {
                AddSections(response.Data);
                Services.HomePageCache.Save(response.Data);
                App.HomePageNeedsRefresh = false;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.FormatMessage());
            }

            EmptyView = "No media found";
            App.HomePageNeedsRefresh = false;
            IsBusy = false;
        }
   
        private void AddSections(HomeScreen hs)
        {
            if (hs.Sections.Count == 0)
            {
                EmptyLabel.Text = "No media found";
                if (!MainStack.Children.Contains(EmptyLabel))
                {
                    MainStack.Children.Clear();
                    MainStack.Children.Add(EmptyLabel);
                }
            }
            else
            {
                if (MainStack.Children.Contains(EmptyLabel))
                    MainStack.Children.Clear();

                //Remove any from MainStack that isn't in Sections
                List<HomePageSectionView> toRemove = new List<HomePageSectionView>();
                foreach (HomePageSectionView v in MainStack.Children)
                    if (!hs.Sections.Select(item => item.ListId).Contains(v.VM.ListId))
                        toRemove.Add(v);
                toRemove.ForEach(v => MainStack.Children.Remove(v));

                //Add new and move around existing
                for (int i = 0; i < hs.Sections.Count; i++)
                {
                    HomePageSectionView existing = MainStack.Children.FirstOrDefault(item => ((HomePageSectionView)item).VM.ListId == hs.Sections[i].ListId) as HomePageSectionView;
                    if (existing == null)
                    {
                        MainStack.Children.Insert(i, new HomePageSectionView(hs.Sections[i]));
                    }
                    else
                    {
                        if (MainStack.Children.IndexOf(existing) != i)
                        {
                            MainStack.Children.Remove(existing);
                            MainStack.Children.Insert(i, existing);
                        }
                        existing.VM.Items.ReplaceRange(hs.Sections[i].Items);

                    }
                }
            }
        }
    }
}