using System.Linq;
using Android.App;
using Android.Content;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    [BroadcastReceiver(Enabled = true, Exported = true), IntentFilter(new[] { Android.App.DownloadManager.ActionDownloadComplete })]
    public class DownloadCompletedBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var reference = intent.GetLongExtra(Android.App.DownloadManager.ExtraDownloadId, -1);

            var download = DownloadManagerImplementation.Current.Queue.Cast<DownloadImplementation>().FirstOrDefault(f => f.Id == reference);
            if (download == null)
                return;

            var query = new Android.App.DownloadManager.Query();
            query.SetFilterById(download.Id);


            using var cursor = ((Android.App.DownloadManager)context.GetSystemService(Context.DownloadService)).InvokeQuery(query);
            while (cursor != null && cursor.MoveToNext())
            {
                DownloadManagerImplementation.Current.UpdateFileProperties(cursor, download);
            }
            cursor?.Close();
        }
    }
}