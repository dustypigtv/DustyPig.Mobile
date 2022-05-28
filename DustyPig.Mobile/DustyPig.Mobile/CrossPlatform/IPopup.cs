using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform
{
    public interface IPopup
    {
        /// <summary>
        /// Alert with a single OK button
        /// </summary>
        Task AlertAsync(string title, string message);

        Task<bool> OkCancelAsync(string title, string message);

        Task<bool> YesNoAsync(string title, string message);
    }
}
