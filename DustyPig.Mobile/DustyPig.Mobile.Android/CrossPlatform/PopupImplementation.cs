using Android.App;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Droid.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(PopupImplemention))]
namespace DustyPig.Mobile.Droid.CrossPlatform
{
    public class PopupImplemention : IPopup
    {
        private static Activity _activity;
        private static int _themeId;

        public static void Init(Activity activity, int themeId)
        {
            _activity = activity;
            _themeId = themeId;
        }

        public Task Alert(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>(); 

            var builder = new Android.Support.V7.App.AlertDialog.Builder(_activity, _themeId);
            builder
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton("OK", (sender, e) =>
                {
                    taskCompletionSource.TrySetResult(true);
                });
            
            var dialog = builder.Create();
            builder.Dispose();

            dialog.Show();

            return taskCompletionSource.Task;
        }

        public Task<bool> OkCancel(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            var builder = new Android.Support.V7.App.AlertDialog.Builder(_activity, _themeId);
            builder
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton("OK", (sender, e) =>
                {
                    taskCompletionSource.TrySetResult(true);
                })
                .SetNegativeButton("Cancel", (sender, e) =>
                {
                    taskCompletionSource.TrySetResult(false);
                });

            var dialog = builder.Create();
            builder.Dispose();

            dialog.Show();

            return taskCompletionSource.Task;
        }

    }
}