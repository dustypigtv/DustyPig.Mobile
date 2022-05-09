using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Search
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : TabbedPage
    {
        public SearchPage()
        {
            InitializeComponent();
            BindingContext = VM = new SearchViewModel();           
        }

        public SearchViewModel VM { get; }

        
        private async void Poster_Tapped(object sender, System.EventArgs e) => await sender.TapEffect();

        private async void CustomSearchHandler_DoQuery(object sender, string e) => await VM.OnDoQuery(e);

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            //HOW does this fire BEFORE the constructor?!
            VM?.OnSizeAllocated(width, height);
        }
    }
}