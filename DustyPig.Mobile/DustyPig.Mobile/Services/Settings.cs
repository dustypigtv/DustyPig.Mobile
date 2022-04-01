using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Services
{
    static class Settings
    {
        private const string LOCAL_ACCOUNT_TOKEN = "local_account_token";
        private const string LOCAL_PROFILE_TOKEN = "local_profile_Token";

        private const string AUTO_SKIP_INTRO_KEY = "auto_skip_intro";
        private const string AUTO_PLAY_NEXT_KEY = "auto_play_next";
        
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


        private static async Task<string> GetSecureString(string id)
        {
            string ret = null;
            try { ret = await SecureStorage.GetAsync(id); }
            catch { }

            if (string.IsNullOrWhiteSpace(ret))
                return null;

            return ret;
        }

        public static Task SaveAccountTokenAsync(string token) => SecureStorage.SetAsync(LOCAL_ACCOUNT_TOKEN, token);

        public static Task<string> GetAccountTokenAsync() => GetSecureString(LOCAL_ACCOUNT_TOKEN);

        public static Task SaveProfileTokenAsync(string token) => SecureStorage.SetAsync(LOCAL_PROFILE_TOKEN, token);

        public static Task<string> GetProfileTokenAsync() => GetSecureString(LOCAL_PROFILE_TOKEN);
    }
}
