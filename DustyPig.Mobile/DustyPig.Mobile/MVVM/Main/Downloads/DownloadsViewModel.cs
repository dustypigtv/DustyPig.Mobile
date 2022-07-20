using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Downloads
{
    internal class DownloadsViewModel : _BaseViewModel
    {
        public DownloadsViewModel(INavigation navigation) : base(navigation)
        {
        }

        public DownloadsViewModel(StackLayout slDimmer, INavigation navigation) : base(slDimmer, navigation)
        {
        }
    }
}
