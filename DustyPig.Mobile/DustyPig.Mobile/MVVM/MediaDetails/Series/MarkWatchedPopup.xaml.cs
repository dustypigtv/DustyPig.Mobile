﻿using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarkWatchedPopup : Popup<MarkWatchedOptions>
    {
        public MarkWatchedPopup()
        {
            InitializeComponent();

            ItemTappedCommand = new Command<MarkWatchedOptions>(o => Dismiss(o));

            BindingContext = this;
        }

        public Command<MarkWatchedOptions> ItemTappedCommand { get; }
    }
}