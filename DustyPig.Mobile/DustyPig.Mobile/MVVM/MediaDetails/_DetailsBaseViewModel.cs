using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public abstract class _DetailsBaseViewModel : _BaseViewModel
    {

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
                        newWidth = height * 0.5;
                        break;
                }
                if (newWidth < 400)
                    newWidth = width;
                Width = newWidth;
            }
       
            ImageHeight = width * 0.5;
        }
    }
}
