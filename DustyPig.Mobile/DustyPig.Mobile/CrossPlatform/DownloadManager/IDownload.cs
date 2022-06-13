namespace DustyPig.Mobile.CrossPlatform.DownloadManager
{
    public interface IDownload
    {
        int MediaId { get; }

        string Url { get; }

        string Suffix { get; }

        string Filename { get; }

        DownloadStatus Status { get; }

        /// <summary>
        /// Gets the status details. F.e. to get the reason why the download failed.
        /// On iOS it's a localized string.
        /// On Android it's the name of the Enum values (Android.App.DownloadError or Android.App.DownloadPausedReason)
        /// as string, prefixed by either `Error` or `Paused` e.g. `Error.HttpDataError` or `Paused.QueuedForWifi`.
        /// If (in any case) you encounter the property `Unregistered:` followed by an integer, please report it.
        /// These are new values for either enumeration. You can find the reason in the official documentation:
        /// https://developer.android.com/reference/android/app/DownloadManager.html
        /// Every error-response (status-code gte 400 and lt 600) is prefixed by `Error.HttpCode: `. Be aware that
        /// some custom codes, may have unexpected results. E.g the number 497 is reserved for the error message
        /// `Error.TooManyRedirects` and 488 would result in the error `Error.FileAlreadyExists`.
        /// </summary>
        /// <value>The status details.</value>
        string StatusDetails { get; }

        long TotalBytesExpected { get; }

        long TotalBytesWritten { get; }

        int Percent { get; }
    }
}
