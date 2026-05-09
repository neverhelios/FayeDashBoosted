using BepInEx;
using BepInEx.Logging;
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
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(FayeDashPatch));
        Harmony.CreateAndPatchAll(typeof(FieldAbilities_Activate_Patch));
    }
    
    [HarmonyPatch(typeof(Field.FieldAbilities))]
    [HarmonyPatch("Activate")]
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
                        nextSafeInstructions = 2;
                        nextRemovedInstructions = 4;
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
    }
    

    [HarmonyPatch(typeof(Field.FieldAbilities), "Activate")]
    class FayeDashPatch
    {
        public static void DrawCurrentSoundIndex()
        {
            // GUI.Label(new Rect(30, 20, 300, 100), "<color=#c0c0c0ff><size=40>Sound index: " + 0 + " </size></color>");
        }

        static void Prefix(out Stopwatch __state, Field.FieldAbilities __instance)
        {
            switch (0)
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

            // MelonEvents.OnGUI.Subscribe(DrawCurrentSoundIndex, 100);

            Logger.LogInfo($"Ca se mad le dash");
            __state = new Stopwatch(); // assign your own state
            __state.Start();

            Type fieldAbilitiesType = typeof(Field.FieldAbilities);
            FieldInfo madDashDurationFieldInfo = fieldAbilitiesType.GetField("madDashDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            Logger.LogInfo("Mad dash duration: " + madDashDurationFieldInfo.GetValue(__instance));
        }

        static void Postfix(Stopwatch __state)
        {
            __state.Stop();
            Logger.LogInfo(__state.Elapsed.ToString());
        }
    }
}

