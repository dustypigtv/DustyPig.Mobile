using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform
{
    public interface IPopup
    {
        /// <summary>
        /// Alert with a single OK button
        /// </summary>
        Task Alert(string title, string message);

        Task<bool> OkCancel(string title, string message);
    }
}
