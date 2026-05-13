using System;
using System.Reflection;
using System.Collections;
using Global.UI;
using UnityEngine;
using Field;

namespace FayeDashBoosted;
class Popups
{
    public static void ShowTreasurePopup(string name, string title, string subtitle, string description, string charMod, bool activateCharMod, string treasureSprite)
    {
        TreasureUI treasureUI = CommonObjects.GetTreasureUI();
        Type treasureUIType = typeof(TreasureUI);

        FieldInfo continueButtonFieldInfo = treasureUIType.GetField("continueButton", BindingFlags.NonPublic | BindingFlags.Instance);
        ((Global.UI.StealFocus)continueButtonFieldInfo.GetValue(treasureUI)).enabled = false;
        treasureUI.window.SetActive(value: true);

        MethodInfo FocusContinueAfterDelayMethodInfo = treasureUIType.GetMethod("FocusContinueAfterDelay", BindingFlags.NonPublic | BindingFlags.Instance);
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