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
using PixelCrushers.DialogueSystem;
using Global.UI;
using Field;

namespace FayeDashBoosted;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static ConfigEntry<String> playedSoundIndexConfig;

    private static ConfigEntry<float> dashDurationConfig;
    private static ConfigEntry<float> dashCooldownConfig;
    private static ConfigEntry<float> dashSpeedConfig;

    private static ConfigEntry<bool> dashRepeatKeyConfig;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        playedSoundIndexConfig = Config.Bind("General.Sound", "PlayedSoundIndex", "classic", "The sound that will be played on dash (More info in the docs)");

        dashDurationConfig = Config.Bind("General.Dash", "DashDuration", 1.0f, "The duration of the dash in seconds");
        dashCooldownConfig = Config.Bind("General.Dash", "DashCooldown", 0.1f, "The cooldown between the dashes in seconds");
        dashSpeedConfig = Config.Bind("General.Dash", "DashSpeed", 3.0f, "The speed mutiplicator of the dash (Warning, if you go higher than four, there is chanes that you miss loading zones triggers)");

        dashRepeatKeyConfig = Config.Bind("General.Dash", "RepeatKey", true, "Set to true if you want to continuously dash by keeping the key down");

        Harmony.CreateAndPatchAll(typeof(FieldAbilities_Activate_Patch));
        Harmony.CreateAndPatchAll(typeof(FieldPlayerControl_CheckAbility_Patch));
        Harmony.CreateAndPatchAll(typeof(FieldPlayer_StartDashing_Patch));
    }
    [HarmonyPatch(typeof(FieldPlayerControl), "CheckAbility")]
    public static class FieldPlayerControl_CheckAbility_Patch
    {
        static bool Prefix(FieldPlayerControl __instance)
        {
            if(dashRepeatKeyConfig.Value)
            {
                Type fieldPlayerControlType = typeof(FieldPlayerControl);
                FieldInfo inputFieldInfo = fieldPlayerControlType.GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);
                if (((Rewired.Player)inputFieldInfo.GetValue(__instance)).GetButton("FieldAbility"))
                {
                    FieldInfo abilitiesFieldInfo = fieldPlayerControlType.GetField("abilities", BindingFlags.NonPublic | BindingFlags.Instance);
                    ((FieldAbilities)abilitiesFieldInfo.GetValue(__instance)).Activate();
                }
                return false; // Skip entierely the original function
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FieldAbilities), "Activate")]
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

        static void Prefix(out Stopwatch __state, FieldAbilities __instance)
        {
            __state = new Stopwatch(); // assign your own state
            __state.Start();

            Type fieldAbilitiesType = typeof(FieldAbilities);

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

            FieldInfo madDashCooldownFieldInfo = fieldAbilitiesType.GetField("madDashCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
            madDashCooldownFieldInfo.SetValue(__instance, dashCooldownConfig.Value);

            FieldInfo madDashSpeedIncreaseFieldInfo = fieldAbilitiesType.GetField("madDashSpeedIncrease", BindingFlags.NonPublic | BindingFlags.Instance);
            madDashSpeedIncreaseFieldInfo.SetValue(__instance, dashSpeedConfig.Value);

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

            // ShowTreasurePopup("C'est la SUITE, de la SUITE", "Quand j'dis Aladin", "Tout le monde dit le prince",  "Je suis passe partout, de fort boyard,\nje guide les casse-cou, dans des traquenards", "Archipelago Item", false, "item-classchange");
        }

        static void Postfix(Stopwatch __state)
        {
            __state.Stop();
            // Logger.LogInfo(__state.Elapsed.ToString());
        }
    }

    [HarmonyPatch(typeof(FieldPlayer), "StartDashing")]
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
                    continue;
                }

                if(nextSafeInstructions > 0)
                    nextSafeInstructions--;

                yield return instruction;
            }
        }
    }

    public static void ListAllItems()
    {
        DialogueSystemController DialogueInstance = DialogueManager.instance;

        foreach(Item curr_item in DialogueInstance.databaseManager.masterDatabase.items)
        {
            string text = curr_item.LookupValue("IconSlot");
            if (!string.IsNullOrEmpty(text))
            {
                Logger.LogInfo("Has icon slot: " + curr_item.Name + "\nWhich is " + text);
            }
            else
            {
                Logger.LogInfo(curr_item.Name);
            }
        }
        Item item = DialogueManager.DatabaseManager.masterDatabase.GetItem("[Treasure] MODPODClass");
        Logger.LogInfo("Item " + item.Name + " found on database");
    }

    public static void ShowTreasurePopup(string name, string title, string subtitle, string description, string charMod, bool activateCharMod, string treasureSprite)
    {
        TreasureUI treasureUI = CommonObjects.GetTreasureUI();
        Type treasureUIType = typeof(TreasureUI);

        FieldInfo continueButtonFieldInfo = treasureUIType.GetField("continueButton", BindingFlags.NonPublic | BindingFlags.Instance);
        ((Global.UI.StealFocus)continueButtonFieldInfo.GetValue(treasureUI)).enabled = false;
        treasureUI.window.SetActive(value: true);

        MethodInfo FocusContinueAfterDelayMethodInfo = treasureUIType.GetMethod("FocusContinueAfterDelay", BindingFlags.NonPublic | BindingFlags.Instance);
        Logger.LogInfo("FocusContinueAfterDelayMethodInfo: " + FocusContinueAfterDelayMethodInfo.ToString());
        treasureUI.StartCoroutine((IEnumerator)FocusContinueAfterDelayMethodInfo.Invoke(treasureUI,  new object[] {}));


        ItemWindow itemWindow = treasureUI.itemWindow;
        UIHelper.SetText(itemWindow.gearName, name);
        UIHelper.SetText(itemWindow.gearType, title);
        Global.UI.UIHelper.SetText(treasureUI.tutorialText, subtitle);
        UIHelper.SetText(itemWindow.effectText, description);

        Type itemWindowType = typeof(ItemWindow);
        itemWindow.HideAllWindows();

        FieldInfo modEligibleCharNameFieldInfo = itemWindowType.GetField("modEligibleCharName", BindingFlags.NonPublic | BindingFlags.Instance);
        UIHelper.SetText((TMPro.TextMeshProUGUI)modEligibleCharNameFieldInfo.GetValue(itemWindow), charMod);
        FieldInfo eligibleCharContainerFieldInfo = itemWindowType.GetField("eligibleCharContainer", BindingFlags.NonPublic | BindingFlags.Instance);
        ((GameObject)eligibleCharContainerFieldInfo.GetValue(itemWindow)).SetActive(activateCharMod);

        UIHelper.SetImageSprite(treasureUI.itemWindow.gearIcon, CommonObjects.GetGlobal().assetLoader.GetGlobalSprite(treasureSprite));
    }

}

