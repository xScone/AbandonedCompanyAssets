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
using UnityEngine.Experimental.GlobalIllumination;
using JetBrains.Annotations;


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
            Cheating = 100
        }

        public void Awake()    
        {
            instance = this;
            //MyAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "abandonedcompanypropertyitems"));

            Item candle = assetCall.bundle.LoadAsset<Item>("Assets/candleItem.asset");

            createItemLight script = candle.spawnPrefab.AddComponent<createItemLight>();

            //Stuff for physics prop
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = candle;

            NetworkPrefabs.RegisterNetworkPrefab(candle.spawnPrefab);
            Utilities.FixMixerGroups(candle.spawnPrefab);
            Items.RegisterScrap(candle,(int) spawnRate.Cheating, (LevelTypes) (-1));

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "this is a candle";
            Items.RegisterShopItem(candle, 1);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
