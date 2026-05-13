using HarmonyLib;
using System;
using UnityEngine;
using PixelCrushers;


namespace FayeDashBoosted;
public class DashCounter : MonoBehaviour
{
    public int currentDashNb;

    public int DoOneDash(bool showAchievements)
    {
        Plugin.Logger.LogInfo($"Dash number {currentDashNb}");
        currentDashNb++;
        if(showAchievements)
        {
            switch(currentDashNb)
            {
                case 10:
                    Popups.ShowTreasurePopup("Welcome !", "Achievement", "10 dashes made", "Thanks for installing this mod !\nYou can configure a lot of things in FayeDashBoosted.cfg,\ndon't hesitate to lookup", "", false, "item-classchange");
                break;
                case 50:
                    Popups.ShowTreasurePopup("Baby dasher", "Achievement", "50 dashes made", "You like to move faster don't you ?\nThat's only the beginning", "", false, "item-classchange");
                break;
                case 100:
                    Popups.ShowTreasurePopup("A quite normal number", "Achievement", "100 dashes made", "I like the way you move,\ncontinue breezing through the map like that !", "", false, "item-classchange");
                break;
                case 300:
                    Popups.ShowTreasurePopup("Addict to speed", "Achievement", "300 dashes made", "Already 300 dashes ?? You must like this dash button !\nI hope you have the repeat key activated, or else your fingers might start to be numb", "", false, "item-classchange");
                break;
                case 1000:
                    Popups.ShowTreasurePopup("Speedrunner", "Achievement", "1000 dashes made", "Wow, that much dedication ? Tell me were you are on the game (neverhelios on discord)\nI can't fathom where you are rn, did you finish the game already ?", "", false, "item-classchange");
                break;
                case 10000:
                    Popups.ShowTreasurePopup("Literally Sonic", "Achievement", "10000 dashes made", "You must really love the game to stay that much on a single savefile !\nAt least you have this achievement if it means something to you x)", "", false, "item-classchange");
                break;
            }
        }
        return currentDashNb;
    }

    public DashCountSaver.Data RecordData()
    {
        return new DashCountSaver.Data
        {
            savedDashNb = currentDashNb
        };
    }

    public void ApplyData(DashCountSaver.Data data)
    {
        currentDashNb = data.savedDashNb;
    }
}

[RequireComponent(typeof(DashCounter))]
public class DashCountSaver : Saver
{

    public class Data
    {
        public int savedDashNb;
    }

    public override void ApplyData(string s)
    {
        DashCounter component = base.GetComponent<DashCounter>();
        // DashCounter component = gameObject.GetComponent<DashCounter>();

        if(s != null)
            component.currentDashNb = Int32.Parse(s);
        else 
            component.currentDashNb = 0;
    }
    
    public override string RecordData()
    {
        var dataToRecord = gameObject.GetComponent<DashCounter>().RecordData();
        return dataToRecord.savedDashNb.ToString();
    }
}


[HarmonyPatch(typeof(Core.CoreInit), "Start")]
public static class CoreInit_Start_Patch
{
    static void Postfix(Core.CoreInit __instance)
    {
        new GameObject("Dash Counter", [typeof(DashCounter), typeof(DashCountSaver)]).transform.parent = __instance.transform;

        Plugin.Logger.LogInfo("CoreInit Started\n\n");
    }
}
