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
        private static int _themeId;

        public static void Init(int themeId) => _themeId = themeId;
        
        public Task AlertAsync(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            var builder = new Android.Support.V7.App.AlertDialog.Builder(MainActivity.Instance, _themeId);
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

        public Task<bool> OkCancelAsync(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            var builder = new Android.Support.V7.App.AlertDialog.Builder(MainActivity.Instance, _themeId);
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


        public Task<bool> YesNoAsync(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            var builder = new Android.Support.V7.App.AlertDialog.Builder(MainActivity.Instance, _themeId);
            builder
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton("Yes", (sender, e) =>
                {
                    taskCompletionSource.TrySetResult(true);
                })
                .SetNegativeButton("No", (sender, e) =>
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