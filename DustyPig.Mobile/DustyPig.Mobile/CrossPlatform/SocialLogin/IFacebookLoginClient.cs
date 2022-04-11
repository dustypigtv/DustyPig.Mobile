using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform.SocialLogin
{
    public interface IFacebookLoginClient
    {
        Task<string> LoginAsync();
    }
}
