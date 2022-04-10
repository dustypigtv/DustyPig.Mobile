using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform.SocialLogin
{
    public interface ISocialLoginClient
    {
        Task<string> LoginAsync();
    }
}
