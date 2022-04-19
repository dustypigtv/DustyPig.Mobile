using DustyPig.API.v3.Models;
using DustyPig.Mobile.Controls;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.VewModels
{
    public class HomeViewModel : _BaseViewModel
    {
        private readonly Grid _mainGrid;

        public HomeViewModel(Grid mainGrid)
        {
            _mainGrid = mainGrid;

            ItemTappedCommand = new AsyncCommand<BasicMedia>(OnItemTapped);
        }

        private AsyncCommand<BasicMedia> ItemTappedCommand { get; }
        private async Task OnItemTapped(BasicMedia basicMedia)
        {

        }

        public async void OnAppearing()
        {
            //To Do: Handle re-appear, and handle occasional check for updates
            if (_mainGrid.Children.Count > 0)
                return;

            var response = await App.API.Media.GetHomeScreenAsync();
            if (response.Success)
            {
                for (int i = 0; i < response.Data.Sections.Count; i++)
                {
                    var section = response.Data.Sections[i];

                    var homePageSection = new HomePageSection(ItemTappedCommand)
                    {
                        ListId = section.ListId,
                        Title = section.Title
                    };
                    homePageSection.MediaItems.AddRange(section.Items);
                    
                    _mainGrid.Children.Add(homePageSection, 0, i);
                }
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.FormatMessage());
            }            
        }
    }
}
