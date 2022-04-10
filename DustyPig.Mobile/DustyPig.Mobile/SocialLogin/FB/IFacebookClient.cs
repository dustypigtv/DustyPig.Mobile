using System.Threading.Tasks;

namespace DustyPig.Mobile.SocialLogin.FB
{
    public interface IFacebookClient
    {
        Task<string> LoginAsync();
    }
}
