using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;
using System;
using DustyPig.API.v3.Models;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class HomeViewModel : _BaseViewModel
    {
        private static event EventHandler<BasicMedia> AddedToWatchlist;
        private static event EventHandler<BasicMedia> RemovedFromWatchlist;
       
        public HomeViewModel(StackLayout mainStack, Label emptyLabel, INavigation navigation) : base(navigation)
        {
            MainStack = mainStack;
            EmptyLabel = emptyLabel;

            RefreshCommand = new AsyncCommand(Update);

            AddedToWatchlist += ItemAddedToWatchlist;
            RemovedFromWatchlist += ItemRemovedFromWatchlist;
            
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


        private void ItemAddedToWatchlist(object sender, BasicMedia e)
        {
            try
            {
                HomePageSectionView section = MainStack.Children.FirstOrDefault(item => ((HomePageSectionView)item).VM.ListId == API.v3.Clients.MediaClient.ID_WATCHLIST) as HomePageSectionView;
                if(section == null)
                {
                    section = new HomePageSectionView(new HomeScreenList
                    {
                        ListId = API.v3.Clients.MediaClient.ID_WATCHLIST,
                        Title = API.v3.Clients.MediaClient.ID_WATCHLIST_TITLE,
                        Items = new List<BasicMedia>()
                    });

                    var ids = new List<long>();
                    ids.Add(API.v3.Clients.MediaClient.ID_WATCHLIST);
                    foreach (HomePageSectionView v in MainStack.Children)
                        ids.Add(v.VM.ListId);
                    ids.Sort();
                    int idx = ids.IndexOf(API.v3.Clients.MediaClient.ID_WATCHLIST);
                    MainStack.Children.Insert(idx, section);
                }

                section.VM.Items.Add(e);
            }
            catch { }
        }

        public static void InvokeAddedToWatchlist(BasicMedia basicMedia) => AddedToWatchlist?.Invoke(null, basicMedia);

        private void ItemRemovedFromWatchlist(object sender, BasicMedia e)
        {
            try
            {
                HomePageSectionView section = MainStack.Children.First(item => ((HomePageSectionView)item).VM.ListId == API.v3.Clients.MediaClient.ID_WATCHLIST) as HomePageSectionView;
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
            var response = await App.API.Media.GetHomeScreenAsync();
            if (response.Success)
            {
                if (response.Data.Sections.Count == 0)
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
                        if (!response.Data.Sections.Select(item => item.ListId).Contains(v.VM.ListId))
                            toRemove.Add(v);
                    toRemove.ForEach(v => MainStack.Children.Remove(v));

                    //Add new and move around existing
                    for(int i = 0; i < response.Data.Sections.Count; i++)
                    {
                        HomePageSectionView existing = MainStack.Children.FirstOrDefault(item => ((HomePageSectionView)item).VM.ListId == response.Data.Sections[i].ListId) as HomePageSectionView;
                        if (existing == null)
                        {
                            MainStack.Children.Insert(i, new HomePageSectionView(response.Data.Sections[i]));
                        }
                        else
                        {
                            if (MainStack.Children.IndexOf(existing) != i)
                            {
                                MainStack.Children.Remove(existing);
                                MainStack.Children.Insert(i, existing);                                
                            }
                            existing.VM.Items.ReplaceRange(response.Data.Sections[i].Items);
                            
                        }
                    }
                }
                               
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
    }
}