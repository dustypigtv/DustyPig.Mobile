using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform.SocialLogin
{
    public interface IGoogleLoginClient
    {
        Task<string> LoginAsync();
    }
}
