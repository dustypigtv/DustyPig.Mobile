﻿using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace DustyPig.Mobile.MVVM.Search
{
    public class SearchViewModel : _BaseViewModel
    {
        private string _lastQuery = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private string _mediaEmptyString = "Search for title";
        public string MediaEmptyString
        {
            get => _mediaEmptyString;
            set => SetProperty(ref _mediaEmptyString, value);
        }

       
        private ObservableRangeCollection<BasicMedia> _availableItems = new ObservableRangeCollection<BasicMedia>();
        public ObservableRangeCollection<BasicMedia> AvailableItems
        {
            get => _availableItems;
            set => SetProperty(ref _availableItems, value);
        }

        private ObservableRangeCollection<BasicTMDB> _otherItems = new ObservableRangeCollection<BasicTMDB>();
        public ObservableRangeCollection<BasicTMDB> OtherItems
        {
            get => _otherItems;
            set => SetProperty(ref _otherItems, value);
        }


        public async Task OnDoQuery(string query)
        {
            query += string.Empty;

            var terms = (StringUtils.NormalizedQueryString(query) + string.Empty).Tokenize();
            if (terms == null)
                terms = new List<string>();
            string formattedQuery = string.Join(" ", terms);

            if (formattedQuery == _lastQuery)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            if (query == string.Empty)
            {
                MediaEmptyString = "Search for title";
                IsBusy = false;
                AvailableItems.Clear();
                OtherItems.Clear();
                return;
            }

            if (query.Trim().Length < 3 || terms.Count < 1)
            {
                MediaEmptyString = "Enter more letters or words";
                IsBusy = false;
                AvailableItems.Clear();
                OtherItems.Clear();
                return;
            }

            
            IsBusy = true;
            MediaEmptyString = string.Empty;
            _lastQuery = formattedQuery;
            var response = await App.API.Media.SearchAsync(query, token);
            if (response.Success)
            {
                MediaEmptyString = "No matches";
                if (response.Data.Available == null)
                    response.Data.Available = new List<BasicMedia>();
                AvailableItems.ReplaceRange(response.Data.Available);

                if (response.Data.OtherTitles == null)
                    response.Data.OtherTitles = new List<BasicTMDB>();
                OtherItems.ReplaceRange(response.Data.OtherTitles);
            }
            else
            {
                if (token.IsCancellationRequested)
                    return;
                
                MediaEmptyString = "Error searching. Please try again";
                AvailableItems.Clear();
                OtherItems.Clear();
            }

            IsBusy = false;
        }
    }
}
