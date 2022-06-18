using DustyPig.Mobile.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.Helpers
{
    static class Alerts
    {
        public static Task ShowAlertAsync(string title, string msg)
        {
            msg += string.Empty;
            if (msg.StartsWith("\"") && msg.EndsWith("\""))
                msg = msg.Trim('"');

            return DependencyService.Get<IPopup>().AlertAsync(title, msg);
        }
    }
}
