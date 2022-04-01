using DustyPig.Mobile.ViewModels;
using DustyPig.Mobile.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            App.SetOrientation(false);

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));


            CurrentItem = new ShellContent { ContentTemplate = new DataTemplate(typeof(LoginPage)) };
        }

    }
}
