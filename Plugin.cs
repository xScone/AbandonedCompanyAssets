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
using Unity.Netcode;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

//Pile POS: 0, 0.06, -0.025
//Pile ROT: -15, 195, -10
//Duo POS: 0, 0.1, -0.025
//Duo ROT: -60, 195, -5
//Single POS: -0.025, 0.1, 0
//Single ROT: 0, 0, -90



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
        public static Item candle = assetCall.bundle.LoadAsset<Item>("Assets/Items/candle/candleItem.asset");
        public static Item glowstick = assetCall.bundle.LoadAsset<Item>("Assets/Items/glowstick/glowstickItem.asset");
        public static Item GlowstickDroppedItem = assetCall.bundle.LoadAsset<Item>("Assets/Items/glowstick/glowstickItem(Dropped).asset");
        public static Item lighter = assetCall.bundle.LoadAsset<Item>("Assets/Items/lighter/Lighter.asset");
        public static Item bulletLighter = assetCall.bundle.LoadAsset<Item>("Assets/Items/bulletLighter/bulletLighter.asset");
        public static GameObject webBurnParticles = assetCall.bundle.LoadAsset<GameObject>("Assets/Items/lighter/webFire.prefab");
        public static Plugin instance;


        
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

            //CobwebCollisionPatch.Init();

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


            //Stuff for physics prop


            



            //Item Stuff
            NetworkPrefabs.RegisterNetworkPrefab(candle.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(glowstick.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(GlowstickDroppedItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(lighter.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(bulletLighter.spawnPrefab);
            Utilities.FixMixerGroups(candle.spawnPrefab);
            Utilities.FixMixerGroups(GlowstickDroppedItem.spawnPrefab);
            Utilities.FixMixerGroups(glowstick.spawnPrefab);
            Utilities.FixMixerGroups(lighter.spawnPrefab);
            Utilities.FixMixerGroups(bulletLighter.spawnPrefab);

            Items.RegisterScrap(GlowstickDroppedItem, (int)spawnRate.Legendary, (LevelTypes)(-1));
            Items.RegisterScrap(candle, (int)spawnRate.Epic, (LevelTypes)(-1));
            Items.RegisterScrap(lighter, (int)spawnRate.Rare, (LevelTypes)(-1));
            Items.RegisterScrap(bulletLighter, (int)spawnRate.Cheating, (LevelTypes)(-1));


            lighter.toolTips = new string[] { "Light Flip Lighter : [LMB]" };
            bulletLighter.toolTips = new string[] { "Light Bullet Lighter : [LMB]" };
            candle.toolTips = new string[] { "Use item : [LMB]" };
            glowstick.toolTips = new string[] { "Drop Glowstick : [LMB]" };
            CandleStuff.minFail = 1;
            CandleStuff.maxFail = 4;

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "The Candle. A cheap but extremely unreliable method of lighting your way. The Company is not responsible for any fires.\n\n";
            TerminalNode node2 = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "A stack of glowing sticks! The glowsticks do not produce much light, however come in VERY handy for keeping track of your path, they wont stay lit long so be quick! Some creatures may be interested in them, however.\n\n";

            



            Items.RegisterShopItem(candle, null, null, node, 7);
            Items.RegisterShopItem(glowstick, null, null, node2, 20);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
