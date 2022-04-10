namespace DustyPig.Mobile.CrossPlatform.Orientation
{
    public interface IScreen
    {
        void ForcePortrait();
        void ForceLandscape();
        void AllowAnyOrientation();
    }
}
