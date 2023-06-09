﻿using DustyPig.Mobile.CrossPlatform.Orientation;
using DustyPig.Mobile.MVVM;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class App : Application
    {
        internal static readonly API.v3.Client API = new API.v3.Client();

        internal static bool IsMainProfile { get; set; }

        internal static bool HomePageNeedsRefresh { get; set; } = true;

        internal static bool LoggedIn { get; set; }

        public App()
        {
            DevExpress.XamarinForms.CollectionView.Initializer.Init();
            
            InitializeComponent();

            DependencyService.Get<IScreen>().SetOrientation(false);
            Services.Download.DownloadService.Init();
            Services.Progress.ProgressService.Init();

            MainPage = new StartupPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
