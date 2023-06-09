﻿using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        readonly Stack<int> _tabHistory = new Stack<int>();

        public MainPage()
        {
            InitializeComponent();
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
        }


        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            _tabHistory.Push(Children.IndexOf(CurrentPage));

            var cp = CurrentPage as NavigationPage;
            if (cp == null)
                return;

            if (cp.RootPage is Search.SearchPage sp)
                sp.PageShown(false);
        }

        protected override bool OnBackButtonPressed()
        {
            //return base.OnBackButtonPressed();

            if (_tabHistory.Count > 1)
            {
                _tabHistory.Pop();
                CurrentPage = Children[_tabHistory.Pop()];
            }

            return true;
        }

        public void OnTabReselected()
        {
            var navPage = CurrentPage as NavigationPage;
            if (navPage == null)
                return;

            var ips = navPage.RootPage as IPageShown;
            if (ips == null)
                return;

            ips.PageShown(true);
        }
    }
}