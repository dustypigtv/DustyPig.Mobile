using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform.FCM
{
    public interface IFCM
    {
        Task<string> GetTokenAsync();
        Task ResetTokenAsync();
    }
}
