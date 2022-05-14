﻿using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public abstract class _DetailsBaseViewModel : _BaseViewModel
    {
        public _DetailsBaseViewModel(INavigation navigation) : base(navigation) 
        {

        }


        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        
        private string _owner;
        public string Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _imageHeight;
        public double ImageHeight
        {
            get => _imageHeight;
            set => SetProperty(ref _imageHeight, value);
        }

        private string _backdropUrl;
        public string BackdropUrl
        {
            get => _backdropUrl;
            set => SetProperty(ref _backdropUrl, value);
        }

        private string _rating;
        public string Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private double _duration;
        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _durationString;
        public string DurationString
        {
            get => _durationString;
            set => SetProperty(ref _durationString, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _remainingString;
        public string RemainingString
        {
            get => _remainingString;
            set => SetProperty(ref _remainingString, value);
        }

        private double _played;
        public double Played
        {
            get => _played;
            set => SetProperty(ref _played, value);
        }

        private string _genres;
        public string Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        private bool _showGenres;
        public bool ShowGenres
        {
            get => _showGenres;
            set => SetProperty(ref _showGenres, value);
        }

        private string _cast;
        public string Cast
        {
            get => _cast;
            set => SetProperty(ref _cast, value);
        }

        private bool _showCast;
        public bool ShowCast
        {
            get => _showCast;
            set => SetProperty(ref _showCast, value);
        }

        private string _directors;
        public string Directors
        {
            get => _directors;
            set => SetProperty(ref _directors, value);
        }

        private bool _showDirectors;
        public bool ShowDirectors
        {
            get => _showDirectors;
            set => SetProperty(ref _showDirectors, value);
        }

        private string _producers;
        public string Producers
        {
            get => _producers;
            set => SetProperty(ref _producers, value);
        }

        private bool _showProducers;
        public bool ShowProducers
        {
            get => _showProducers;
            set => SetProperty(ref _showProducers, value);
        }

        private string _writers;
        public string Writers
        {
            get => _writers;
            set => SetProperty(ref _writers, value);
        }

        private bool _showWriters;
        public bool ShowWriters
        {
            get => _showWriters;
            set => SetProperty(ref _showWriters, value);
        }

        private bool _showPlayedBar = false;
        public bool ShowPlayedBar
        {
            get => _showPlayedBar;
            set => SetProperty(ref _showPlayedBar, value);
        }

        private string _playButtonText;
        public string PlayButtonText
        {
            get => _playButtonText;
            set => SetProperty(ref _playButtonText, value);
        }

        private string _watchlistIcon;
        public string WatchlistIcon
        {
            get => _watchlistIcon;
            set => SetProperty(ref _watchlistIcon, value);
        }       

        public void OnSizeAllocated(double width, double height)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                Width = width;
            }
            else
            {
                double newWidth = width;
                switch (DeviceDisplay.MainDisplayInfo.Orientation)
                {
                    case DisplayOrientation.Landscape:
                        newWidth = width * 0.5;
                        break;

                    case DisplayOrientation.Portrait:
                        newWidth = width * 0.8;
                        break;
                }
                if (newWidth < 400)
                    newWidth = width;

                Width = (int)Math.Min(width, newWidth);
            }
       
            ImageHeight = (int)(Width * 0.5625);
        }
    }
}