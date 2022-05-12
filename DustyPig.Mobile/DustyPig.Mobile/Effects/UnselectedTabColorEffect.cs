using Xamarin.Forms;

namespace DustyPig.Mobile.Effects
{
    public class UnselectedTabColorEffect : RoutingEffect
    {
        public UnselectedTabColorEffect() : base($"AppEffects.{nameof(UnselectedTabColorEffect)}") { }
    }
}
