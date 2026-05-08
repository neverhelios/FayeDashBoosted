using HarmonyLib;
using MelonLoader;
using System.Diagnostics;
using UnityEngine;

[assembly: MelonInfo(typeof(FayeDashBoosted.Core), "FayeDashBoosted", "1.0.0", "NeverHeliOS", null)]
[assembly: MelonGame("FiveHouses", "Deathbulge")]


namespace FayeDashBoosted
{
    
    public class Core : MelonMod
    {

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Mad Dash Booster Loaded");
        }
    }

    [HarmonyPatch(typeof(Field.FieldAbilities), "Activate")]
    class FayeDashPatch
    {
        private static int MadDashSoundIndex = 0;

        public static void DrawCurrentSoundIndex()
        {
            GUI.Label(new Rect(30, 20, 300, 100), "<color=#c0c0c0ff><size=40>Sound index: " + MadDashSoundIndex + " </size></color>");
        }

        static void Prefix(out Stopwatch __state)
        {
            switch (MadDashSoundIndex)
            {
                case 0:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.confirmSound);
                    break;
                case 1:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.cancelSound);
                    break;
                case 2:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.menuSound);
                    break;
                case 3:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.merchUseSound);
                    break;
                case 4:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.beatBoostSound);
                    break;
                case 5:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.purchaseSound);
                    break;
                case 6:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.failSound);
                    break;
                case 7:
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.equipSound);
                    break;

            }
            
            MelonEvents.OnGUI.Subscribe(DrawCurrentSoundIndex, 100);
            
            MadDashSoundIndex++;
            if (MadDashSoundIndex > 7)
                MadDashSoundIndex = 0;
            //.menuAudio.beatBoostSound);
            Melon<Core>.Logger.Msg("Ca se mad le dash");
            __state = new Stopwatch(); // assign your own state
            __state.Start();
        }

        static void Postfix(Stopwatch __state)
        {
            __state.Stop();
            Melon<Core>.Logger.Msg(__state.Elapsed.ToString());
        }
    }

    [HarmonyPatch(typeof(Field.FieldAbilities))]
    [HarmonyPatch("Activate")]
    public static class FieldAbilities_Activate_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var SoundEventStartIndex = -1;
            var SoundEventEndIndex = -1;

            // Remove the `CommonObjects.GetFMODSoundEvents().PlayEvent(this.madDashFMOD);` call
            // which is after the second `madDashCooldown`

            var currentMadDashCooldown = 0;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand != null && codes[i].operand.ToString() == "System.Single madDashCooldown")
                {
                    currentMadDashCooldown++;
                    if (currentMadDashCooldown == 2)
                    {
                        SoundEventStartIndex = i + 2;
                        SoundEventEndIndex = SoundEventStartIndex + 3;
                    }
                    Melon<Core>.Logger.Msg("Index of cooldown: " + i);
                }
                Melon<Core>.Logger.Msg(codes[i].ToString());
            }
            if (SoundEventStartIndex > -1 && SoundEventEndIndex > -1)
            {
                codes.RemoveRange(SoundEventStartIndex, 4);
            }
            return codes.AsEnumerable();
        }
    }
}