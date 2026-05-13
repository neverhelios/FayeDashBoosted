using HarmonyLib;
using System;
using UnityEngine;
using PixelCrushers;
using FayeDashBoosted;


// TODO: Add "Achievement"
public class DashCounter : MonoBehaviour
{
    public int currentDashNb;

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
