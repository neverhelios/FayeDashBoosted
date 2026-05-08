using MelonLoader;

[assembly: MelonInfo(typeof(FayeDashBoosted.Core), "FayeDashBoosted", "1.0.0", "Elias", null)]
[assembly: MelonGame("FiveHouses", "Deathbulge")]

namespace FayeDashBoosted
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}