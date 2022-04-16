using DustyPig.Mobile.CrossPlatform.Orientation;
using DustyPig.Mobile.MVVM.Auth.Views;
using DustyPig.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class AppShell : Shell
    {
        private static readonly Type[] TabPageTypes = new Type[]
        {
            typeof(HomePage),
            typeof(MoviesPage),
            typeof(SeriesPage),
            typeof(DownloadsPage),
            typeof(SettingsPage)
        };

        private static readonly Stack<int> TabPageHistory = new Stack<int>();


        public AppShell()
        {
            InitializeComponent();
            DependencyService.Get<IScreen>().SetOrientation(false);

            Routing.RegisterRoute(nameof(SignupPage), typeof(SignupPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(SelectProfilePage), typeof(SelectProfilePage));


        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            if (CurrentItem is TabBar tb)
            {
                //Since I'm not using the Id attribute anywhere else
                if (string.IsNullOrEmpty(tb.AutomationId))
                {
                    tb.AutomationId = "Event_Set";
                    TabPageHistory.Clear();
                    TabPageHistory.Push(0);
                    tb.PropertyChanged += TabBar_PropertyChanged;
                }
            }
        }

        private void TabBar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem")
            {
                int index = 0;
                switch (CurrentItem.CurrentItem.CurrentItem.Route)
                {
                    case "Movies":
                        index = 1;
                        break;

                    case "Series":
                        index = 2;
                        break;

                    case "Downloads":
                        index = 3;
                        break;

                    case "Settings":
                        index = 4;
                        break;
                }

                TabPageHistory.Push(index);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (CurrentItem.Route == "Main")
            {
                if (TabPageTypes.Contains(CurrentPage.GetType()))
                {
                    var tabBar = CurrentItem as TabBar;

                    if (TabPageHistory.Count > 1)
                    {
                        //Remove the current page
                        TabPageHistory.Pop();

                        //The top of the stack is now where we need to navigate to
                        //When we do it, the PropertyChanged event will handle
                        //putting it back on the stack
                        int index = TabPageHistory.Pop();

                        tabBar.CurrentItem = tabBar.Items[index];
                    }

                    return true;
                }
            }


            return base.OnBackButtonPressed();
        }


    }
}
