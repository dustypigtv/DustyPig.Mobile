/*
 Based on https://github.com/CrossGeeks/GoogleClientPlugin
*/
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleLoginClientImplementation))]
namespace DustyPig.Mobile.CrossPlatform.SocialLogin
{
    public class GoogleLoginClientImplementation : IGoogleLoginClient
    {
        public Task<string> LoginAsync()
        {
            throw new NotImplementedException();
        }
    }
}