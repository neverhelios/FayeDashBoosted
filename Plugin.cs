using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace FayeDashBoosted;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static ConfigEntry<String> playedSoundIndexConfig;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        playedSoundIndexConfig = Config.Bind("General.Sound", "PlayedSoundIndex", "classic", "The sound that will be played on dash (More info in the docs)");

        Harmony.CreateAndPatchAll(typeof(FieldAbilities_Activate_Patch));
    }

    [HarmonyPatch(typeof(Field.FieldAbilities), "Activate")]
    public static class FieldAbilities_Activate_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Remove the `CommonObjects.GetFMODSoundEvents().PlayEvent(this.madDashFMOD);` call
            // which is after the second `madDashCooldown`

            Logger.LogInfo($"Je voulais juste transpiler");

            var currentMadDashCooldown = 0;
            var nextSafeInstructions = 0;
            var nextRemovedInstructions = 0;

            foreach (var instruction in instructions)
            {
                if (instruction.operand != null && instruction.operand.ToString() == "System.Single madDashCooldown")
                {
                    currentMadDashCooldown++;
                    if (currentMadDashCooldown == 2)
                    {
                        if(playedSoundIndexConfig.Value != "classic")
                        {
                            nextSafeInstructions = 2;
                            nextRemovedInstructions = 4;
                        }
                        else
                        {
                            Logger.LogInfo("EN FAIT NON");
                        }
                    }
                }

                if (nextRemovedInstructions > 0 && nextSafeInstructions <= 0)
                {
                    nextRemovedInstructions--;
                    continue;
                }

                if(nextSafeInstructions > 0)
                    nextSafeInstructions--;

                yield return instruction;
            }
        }

        static void Prefix(out Stopwatch __state, Field.FieldAbilities __instance)
        {
            Type fieldAbilitiesType = typeof(Field.FieldAbilities);
            switch(playedSoundIndexConfig.Value)
            {
                case "confirmSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.confirmSound);
                    break;
                case "cancelSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.cancelSound);
                    break;
                case "merchUseSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.merchUseSound);
                    break;
                case "beatBoostSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.beatBoostSound);
                    break;
                case "purchaseSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.purchaseSound);
                    break;
                case "failSound":
                    CommonObjects.GetFMODSoundEvents().PlayEvent(CommonObjects.GetShopsMenuUI().menuAudio.failSound);
                    break;
                case "slink":
                    FieldInfo slinkSoundFieldInfo = fieldAbilitiesType.GetField("slinkFMOD", BindingFlags.NonPublic | BindingFlags.Instance);
                    CommonObjects.GetFMODSoundEvents().PlayEvent((string)slinkSoundFieldInfo.GetValue(__instance));
                    break;
                case "slinkFailure":
                    FieldInfo slinkFailureSoundFieldInfo = fieldAbilitiesType.GetField("slinkFailureFMODNick", BindingFlags.NonPublic | BindingFlags.Instance);
                    CommonObjects.GetFMODSoundEvents().PlayEvent((string)slinkFailureSoundFieldInfo.GetValue(__instance));
                    break;
                case "shockwave":
                    CommonObjects.GetFMODSoundEvents().PlayEvent("Shockwave");
                    break;
                case "kwak-double-scratch":
                    CommonObjects.GetFMODSoundEvents().PlayEvent("KwakDoubleScratch");
                    break;
                case "kwak-clap":
                    CommonObjects.GetFMODSoundEvents().PlayEvent("KwakClap");
                    break;
                case "classic":
                    break;
            }

            Logger.LogInfo($"Ca se mad le dash");
            __state = new Stopwatch(); // assign your own state
            __state.Start();

            FieldInfo madDashDurationFieldInfo = fieldAbilitiesType.GetField("madDashDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            Logger.LogInfo("Mad dash duration: " + madDashDurationFieldInfo.GetValue(__instance));
        }

        static void Postfix(Stopwatch __state)
        {
            __state.Stop();
            // Logger.LogInfo(__state.Elapsed.ToString());
        }
    }
}

