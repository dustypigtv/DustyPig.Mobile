// https://www.damirscorner.com/blog/posts/20210416-AwaitingGooglePlayServicesTasks.html

using Android.Gms.Tasks;

namespace System.Threading.Tasks
{
    internal static class Extensions
    {
        public static Task<Java.Lang.Object> ToAwaitableTask(this Android.Gms.Tasks.Task task)
        {
            var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
            var taskCompleteListener = new TaskCompleteListener(taskCompletionSource);
            task.AddOnCompleteListener(taskCompleteListener);
            return taskCompletionSource.Task;
        }

        private class TaskCompleteListener : Java.Lang.Object, IOnCompleteListener
        {
            private readonly TaskCompletionSource<Java.Lang.Object> _taskCompletionSource;

            public TaskCompleteListener(TaskCompletionSource<Java.Lang.Object> tcs) => _taskCompletionSource = tcs;

            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (task.IsCanceled)
                {
                    _taskCompletionSource.TrySetCanceled();
                }
                else if (task.IsSuccessful)
                {
                    _taskCompletionSource.TrySetResult(task.Result);
                }
                else
                {
                    _taskCompletionSource.TrySetException(task.Exception);
                }
            }
        }
    }
}