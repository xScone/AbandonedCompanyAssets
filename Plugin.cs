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
using AbandonedCompanyAssets.itemStuff;
using AbandonedCompanyAssets.Patches;
using UnityEngine.Yoga;
using Steamworks.Ugc;
using UnityEngine.UIElements;


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
        public static Item candle = assetCall.bundle.LoadAsset<Item>("Assets/Working Shit/candleItem.asset");
        public static Item glowstick = assetCall.bundle.LoadAsset<Item>("Assets/Working Shit/glowstick.asset");
        public static GameObject droppableGlowstick = assetCall.bundle.LoadAsset<GameObject>("Assets/Working Shit/droppableGlowstick.prefab");
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
            Cheating = 1000,
            NonSpawning = 0
        }

        public void Awake()    
        {
            instance = this;
            ACALog = Logger;

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            CandleStuff candleStuff = candle.spawnPrefab.AddComponent<CandleStuff>();
            FlickeringLight script2 = candle.spawnPrefab.AddComponent<FlickeringLight>();
            GlowstickStuff glowstickStuff = glowstick.spawnPrefab.AddComponent<GlowstickStuff>();
            GlowstickStuff droppableGlowstickStuff = droppableGlowstick.AddComponent<GlowstickStuff>();




            //Stuff for physics prop
            candleStuff.grabbable = true;
            candleStuff.grabbableToEnemies = true;
            candleStuff.useCooldown = 0.8f;
            candleStuff.itemProperties = candle;

            glowstickStuff.itemProperties = glowstick;
            glowstickStuff.grabbable = true;
            glowstickStuff.grabbableToEnemies = true;
            glowstickStuff.useCooldown = 0.8f;

            GlowstickStuff.clonePrefab = droppableGlowstick;
            droppableGlowstickStuff.name = "Glowstick";


            droppableGlowstickStuff.grabbable = false;
            droppableGlowstickStuff.grabbableToEnemies = true;
            droppableGlowstickStuff.scrapValue = 0;
            droppableGlowstickStuff.itemProperties = glowstick;
            droppableGlowstickStuff.fallTime = 5f;


            //Item Stuff
            NetworkPrefabs.RegisterNetworkPrefab(candle.spawnPrefab);
            Utilities.FixMixerGroups(candle.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(glowstick.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(GlowstickStuff.clonePrefab);
            Utilities.FixMixerGroups(glowstick.spawnPrefab);
            Items.RegisterScrap(candle,(int) spawnRate.Rare, (LevelTypes) (-1));
            Items.RegisterScrap(glowstick, (int)spawnRate.NonSpawning, (LevelTypes)(-1));
            candle.toolTips = new string[] { "Use item : [LMB]" };
            CandleStuff.minFail = 2;
            CandleStuff.maxFail = 5;

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "The Candle. A cheap but extremely unreliable method of lighting your way. The Company is not responsible for any fires.\n\n";
            TerminalNode node2 = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "glowstick lol";



            Items.RegisterShopItem(candle, null, null, node, 7);
            Items.RegisterShopItem(glowstick, 1);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
