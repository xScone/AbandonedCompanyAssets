using BepInEx;
using BepInEx.Logging;
using LethalLib.Modules;
using HarmonyLib;
using System;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib.Tools;
using LethalLib;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static LethalLib.Modules.Levels;
using AbandonedCompanyAssets.Behaviours;
using AbandonedCompanyAssets.Patches;
using UnityEngine.Yoga;
using Steamworks.Ugc;


namespace AbandonedCompanyAssets
{
    public static class Configs
    {
        public static bool spawnEquipmentInFacility = true;
        public static int maxEquipmentSpawns = 8;
        public static int minEquipmentSpawns = 2;
    }
    public static class assetCall 
    {
        static string assetFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "abandonedcompanypropertyitems");
        public static AssetBundle bundle = AssetBundle.LoadFromFile(assetFile);
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource ACALog;
        public static Item candle = assetCall.bundle.LoadAsset<Item>("Assets/candleItem.asset");
        private readonly Harmony harmony = new Harmony("AbandonedCompanyAssets");
        public static Plugin instance;
        //public static AssetBundle MyAssets;


        
        private enum spawnRate
        {
            Legendary = 5,
            Epic = 10,
            Rare = 20,
            Uncommon = 30,
            Common = 40,
            Extremely_Common = 50,
            Too_Common = 75,
            Cheating = 1000
        }

        public void Awake()    
        {
            instance = this;

            ACALog = Logger;
            createItemLight script = candle.spawnPrefab.AddComponent<createItemLight>();
            flickeringLight script2 = candle.spawnPrefab.AddComponent<flickeringLight>();
            

            //Stuff for physics prop
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.useCooldown = 0.8f;
            
            script.itemProperties = candle;
            
            NetworkPrefabs.RegisterNetworkPrefab(candle.spawnPrefab);
            Utilities.FixMixerGroups(candle.spawnPrefab);
            Items.RegisterScrap(candle,(int) spawnRate.Rare, (LevelTypes) (-1));
            candle.toolTips = new string[] { "Use item : [LMB]" };
            createItemLight.minFail = 2;
            createItemLight.maxFail = 5;

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "The Candle. A cheap but extremely unreliable method of lighting your way. The Company is not responsible for any fires.\n\n";
            Items.RegisterShopItem(candle, null, null, node, 7);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
