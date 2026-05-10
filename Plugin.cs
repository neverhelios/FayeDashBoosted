using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;
using UnityEngine;

namespace FayeDashBoosted;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static ConfigEntry<String> playedSoundIndexConfig;
    private static ConfigEntry<float> dashDurationConfig;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        playedSoundIndexConfig = Config.Bind("General.Sound", "PlayedSoundIndex", "classic", "The sound that will be played on dash (More info in the docs)");
        dashDurationConfig = Config.Bind("General.Dash", "DashDuration", 1.0f, "The duration of the dash in seconds");

        Harmony.CreateAndPatchAll(typeof(FieldAbilities_Activate_Patch));
        Harmony.CreateAndPatchAll(typeof(FieldPlayer_StartDashing_Patch));
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
            __state = new Stopwatch(); // assign your own state
            __state.Start();

            Type fieldAbilitiesType = typeof(Field.FieldAbilities);

            MethodInfo canUseMethodInfo = fieldAbilitiesType.GetMethod("CanUse", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo partyFieldInfo = fieldAbilitiesType.GetField("party", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!(bool)canUseMethodInfo.Invoke(__instance, new object[] {}) || ((Core.Party)partyFieldInfo.GetValue(__instance)).ActiveChar().characterIndex != 0)
            {
                return;
            }

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
                    // Overkill but allows me to rembember how to use reflection
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


            FieldInfo madDashDurationFieldInfo = fieldAbilitiesType.GetField("madDashDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            madDashDurationFieldInfo.SetValue(__instance, dashDurationConfig.Value);

            FieldInfo blinkStartTimeFieldInfo = fieldAbilitiesType.GetField("blinkStartTime", BindingFlags.NonPublic | BindingFlags.Instance);
            blinkStartTimeFieldInfo.SetValue(__instance, (float)madDashDurationFieldInfo.GetValue(__instance) * 0.75f);

            MethodInfo applySlinkMethodInfo = fieldAbilitiesType.GetMethod("ApplySlink", BindingFlags.NonPublic | BindingFlags.Instance);

            // Idk why i can't find this function so I'll do it the bruteforce way but I really should go back on this later (this wont be the case but a man can dream)
            // MethodInfo startCoroutineMethodInfo = baseType.GetMethod("StartCoroutine", BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(System.Collections.IEnumerator)}, null);

            Type baseType = fieldAbilitiesType.BaseType;
            MethodInfo startCoroutineMethodInfo = null;
            MethodInfo[] allBaseMethods = baseType.GetMethods();
            foreach(var baseMethod in allBaseMethods)
            {
                if(baseMethod.ToString() == "UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator)")
                    startCoroutineMethodInfo = baseMethod;
            }

            // Equivalent to: base.StartCoroutine(this.ApplySlink(this.madDashDuration))
            startCoroutineMethodInfo.Invoke(__instance, new object[] {applySlinkMethodInfo.Invoke(__instance, new object[] {madDashDurationFieldInfo.GetValue(__instance)})});

            Logger.LogInfo("Mad dash duration: " + madDashDurationFieldInfo.GetValue(__instance));
            Logger.LogInfo("Blink start time: " + blinkStartTimeFieldInfo.GetValue(__instance));
        }

        static void Postfix(Stopwatch __state)
        {
            __state.Stop();
            // Logger.LogInfo(__state.Elapsed.ToString());
        }
    }

    [HarmonyPatch(typeof(Field.FieldPlayer), "StartDashing")]
    public static class FieldPlayer_StartDashing_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Remove the `this.Control.enabled = false;` instruction

            var currentCurrentSpeedInstruction = 0;
            var nextSafeInstructions = 0;
            var nextRemovedInstructions = 0;

            foreach (var instruction in instructions)
            {
                if (instruction.operand != null && instruction.operand.ToString() == "System.Single currentSpeed")
                {
                    currentCurrentSpeedInstruction++;
                    if (currentCurrentSpeedInstruction == 1)
                    {
                        nextSafeInstructions = 1;
                        nextRemovedInstructions = 4;
                    }
                }

                if (nextRemovedInstructions > 0 && nextSafeInstructions <= 0)
                {
                    nextRemovedInstructions--;
                    Logger.LogInfo("Removed: " + instruction.ToString());
                    continue;
                }

                if(nextSafeInstructions > 0)
                    nextSafeInstructions--;

                yield return instruction;
            }
        }
    }
}

