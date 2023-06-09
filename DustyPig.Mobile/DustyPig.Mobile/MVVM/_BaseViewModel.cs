﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.MVVM.MediaDetails.Movie;
using DustyPig.Mobile.MVVM.MediaDetails.Playlist;
using DustyPig.Mobile.MVVM.MediaDetails.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM
{
    public abstract class _BaseViewModel : INotifyPropertyChanged
    {
        public event EventHandler<bool> InternetConnectivityChanged;

        private readonly StackLayout _slDimmer = null;

        public _BaseViewModel(INavigation navigation)
        {
            Navigation = navigation;

            NoInternet = (int)Connectivity.NetworkAccess < 3;
            Connectivity.ConnectivityChanged += (sender, e) =>
            {
                NoInternet = (int)e.NetworkAccess < 3;
                InternetConnectivityChanged?.Invoke(this, !NoInternet);
            };

            ItemTappedCommand = new AsyncCommand<BasicMedia>(OnItemTapped);
            TMDBItemTappedCommand = new AsyncCommand<BasicTMDB>(OnTMDBItemTapped);
        }

        public _BaseViewModel(StackLayout slDimmer, INavigation navigation) : this(navigation)
        {
            _slDimmer = slDimmer;
        }




        public INavigation Navigation { get; }

        public Task ShowAlertAsync(string title, string msg) => Helpers.Alerts.ShowAlertAsync(title, msg);

        bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _noInternet;
        public bool NoInternet
        {
            get => _noInternet;
            set => SetProperty(ref _noInternet, value);
        }

        public AsyncCommand<BasicMedia> ItemTappedCommand { get; }
        private async Task OnItemTapped(BasicMedia item)
        {
            switch (item.MediaType)
            {
                case MediaTypes.Movie:
                    await Navigation.PushModalAsync(new MovieDetailsPage(item, _slDimmer));
                    break;

                case MediaTypes.Series:
                    await Navigation.PushModalAsync(new SeriesDetailsPage(item, _slDimmer));
                    break;

                case MediaTypes.Playlist:
                    await Navigation.PushModalAsync(new PlaylistDetailsPage(item, _slDimmer));
                    break;

                default:
                    await ShowAlertAsync("Tapped", item.Title);
                    break;
            }
        }

        public AsyncCommand<BasicTMDB> TMDBItemTappedCommand { get; }
        private async Task OnTMDBItemTapped(BasicTMDB item)
        {
            await Navigation.PushModalAsync(new MediaDetails.TMDB.TMDBDetailsPage(item, _slDimmer));
        }



        protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
