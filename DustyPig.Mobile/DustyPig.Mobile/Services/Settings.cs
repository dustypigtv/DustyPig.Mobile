using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Services
{
    static class Settings
    {
        private const string PROFILE_TOKEN = "profile_Token";

        private const string AUTO_SKIP_INTRO_KEY = "auto_skip_intro";
        private const string AUTO_PLAY_NEXT_KEY = "auto_play_next";

        private const string DOWNLOAD_OVER_CELLULAR_KEY = "download_over_cellular";

        private const string LAST_DOWNLOAD_COUNT = "last_download_count";

        public static bool AutoPlayNext
        {
            get => Preferences.Get(AUTO_PLAY_NEXT_KEY, false);
            set => Preferences.Set(AUTO_PLAY_NEXT_KEY, value);
        }

        public static bool AutoSkipIntro
        {
            get => Preferences.Get(AUTO_SKIP_INTRO_KEY, false);
            set => Preferences.Set(AUTO_SKIP_INTRO_KEY, value);
        }

        public static bool DownloadOverCellular
        {
            get => Preferences.Get(DOWNLOAD_OVER_CELLULAR_KEY, false);
            set => Preferences.Set(DOWNLOAD_OVER_CELLULAR_KEY, value);
        }

        public static int LastDownloadCount
        {
            get => int.Parse(Preferences.Get(LAST_DOWNLOAD_COUNT, "5"));
            set => Preferences.Set(LAST_DOWNLOAD_COUNT, value.ToString());
        }


        private static async Task<string> GetSecureString(string id)
        {
            string ret = null;
            try { ret = await SecureStorage.GetAsync(id); }
            catch { }

            if (string.IsNullOrWhiteSpace(ret))
                return null;

            return ret;
        }

        public static Task SaveProfileTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult(SecureStorage.Remove(PROFILE_TOKEN));
            else
                return SecureStorage.SetAsync(PROFILE_TOKEN, token);
        }


        public static Task<string> GetProfileTokenAsync() => GetSecureString(PROFILE_TOKEN);

        public static void DeleteProfileToken() => SecureStorage.Remove(PROFILE_TOKEN);
    }
}
