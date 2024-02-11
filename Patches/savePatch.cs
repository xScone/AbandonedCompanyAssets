using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using AbandonedCompanyAssets;
using GameNetcodeStuff;
using AbandonedCompanyAssets.Behaviours;
using JetBrains.Annotations;

namespace AbandonedCompanyAssets.Patches
{
    /*/internal class savePatch
    {
       public static class dataSave
        {
            public static List<int> failList = new List<int>();
            
        }
        [HarmonyPatch(nameof(StartOfRound.Start))]
        [HarmonyPostfix]

        //this "works" but only saves data for the single variables and then applies them item wide, not a solution to candles but a good thing to know nevertheless.

        private static void UpdatePostfix(StartOfRound __instance)
        {
           
            if (ES3.KeyExists("currentlyLit"))
            {
                createItemLight.currentlyLit = ES3.Load("currentlyLit", false);
            }
            else
            {
                ES3.Save("currentlyLit", createItemLight.currentlyLit);
            }
            if (ES3.KeyExists("totalFailCount"))
            {
                createItemLight.totalFailCount = ES3.Load("totalFailCount", 0);
            }
            else
            {
                ES3.Save("totalFailCount", createItemLight.minFail);
            }
            if (ES3.KeyExists("minFail"))
            {
                createItemLight.minFail = ES3.Load("minFail", 5);
            }
            else
            {
                ES3.Save("minFail", createItemLight.minFail);
            }
            if (ES3.KeyExists("maxFail"))
            {
                createItemLight.maxFail = ES3.Load("maxFail", 15);
            }
            else
            {
                ES3.Save("maxFail", createItemLight.maxFail);
            }
        }
    }/*/

}
