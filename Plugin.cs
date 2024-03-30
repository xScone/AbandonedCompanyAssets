using BepInEx;
using BepInEx.Logging;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Reflection;
using static LethalLib.Modules.Levels;
using AbandonedCompanyAssets.itemStuff;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;
using System.Transactions;



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
        public static Item signalFlareItem = assetCall.bundle.LoadAsset<Item>("Assets/Items/emergencyFlare/signalFlareItem.asset");
        public static GameObject signalFlareParticles = assetCall.bundle.LoadAsset<GameObject>("Assets/Items/emergencyFlare/signalFlare (1) 1.prefab");
		public static Item abandonedflashlightitem = assetCall.bundle.LoadAsset<Item>("Assets/Items/bbflashlight/abandonedBBFlashlight.asset");
		public static Item industrialflashlightitem = assetCall.bundle.LoadAsset<Item>("Assets/Items/industrialflashlight/industrialFlashlightItem.asset");
		public static Item proabaondedflashlightitem = assetCall.bundle.LoadAsset<Item>("Assets/Items/proflashlight/ProAbandonedFlashlight.asset");
		public static Item abandonedWalkieItem = assetCall.bundle.LoadAsset<Item>("Assets/Items/AbandonedWalkie/AbandonedWalkieItem.asset");
		public static Item abandonedTZPItem = assetCall.bundle.LoadAsset<Item>("Assets/Items/TZP-Abandoned/AbandonedTZPItem.asset");
        public static Plugin instance;

		private static bool EnableFlare = true;
		private static bool EnableGlowsticks = true;
		private static bool EnableRandomGlowsticks = true;
		private static bool EnableLighter = true;
		private static bool EnableDungeonCandle = true;
		private static bool EnableShopCandle = true;
		private static bool EnableAbandonedFlashlight = true;
		private static bool FlareCompatName = false;
		private static bool EnableIndustrialFlashlight = true;
		private static bool EnableProAbandonedFlashlight = true;
		private static bool EnableAbandonedWalkie = true;
		private static bool EnableAbandonedTZP = true;
		private static int CandleSpawnWeight = 10;
		private static int LighterSpawnWeight = 20;
		private static int BulletLighterSpawnWeight = 2;
		private static int AbandonedBBFlashSpawnWeight = 30;
		private static int AbandonedProFlashSpawnWeight = 20;
		private static int IndustrialFlashlightSpawnWeight = 5;
		private static int AbandonedWalkieSpawnWeight = 20;
		private static int AbandonedTZPSpawnWeight = 60;
		private static int DroppedGlowstickSpawnWeight = 10;






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

			loadConfig();

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

            //Item Stuff
            NetworkPrefabs.RegisterNetworkPrefab(candle.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(glowstick.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(GlowstickDroppedItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(lighter.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(webBurnParticles);
            NetworkPrefabs.RegisterNetworkPrefab(bulletLighter.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(signalFlareItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(signalFlareParticles);
			NetworkPrefabs.RegisterNetworkPrefab(abandonedflashlightitem.spawnPrefab);
			NetworkPrefabs.RegisterNetworkPrefab(industrialflashlightitem.spawnPrefab);
			NetworkPrefabs.RegisterNetworkPrefab(proabaondedflashlightitem.spawnPrefab);
			NetworkPrefabs.RegisterNetworkPrefab(abandonedWalkieItem.spawnPrefab);
			NetworkPrefabs.RegisterNetworkPrefab(abandonedTZPItem.spawnPrefab);

			Utilities.FixMixerGroups(candle.spawnPrefab);
            Utilities.FixMixerGroups(GlowstickDroppedItem.spawnPrefab);
            Utilities.FixMixerGroups(glowstick.spawnPrefab);
            Utilities.FixMixerGroups(lighter.spawnPrefab);
            Utilities.FixMixerGroups(bulletLighter.spawnPrefab);
            Utilities.FixMixerGroups(bulletLighter.spawnPrefab);
            Utilities.FixMixerGroups(signalFlareItem.spawnPrefab);
            Utilities.FixMixerGroups(webBurnParticles);
            Utilities.FixMixerGroups(signalFlareParticles);
			Utilities.FixMixerGroups(abandonedflashlightitem.spawnPrefab);
			Utilities.FixMixerGroups(industrialflashlightitem.spawnPrefab);
			Utilities.FixMixerGroups(proabaondedflashlightitem.spawnPrefab);
			Utilities.FixMixerGroups(abandonedWalkieItem.spawnPrefab);
			Utilities.FixMixerGroups(abandonedTZPItem.spawnPrefab);

			if (EnableGlowsticks) { Items.RegisterScrap(GlowstickDroppedItem, DroppedGlowstickSpawnWeight, (LevelTypes)(-1)); }
            if (EnableDungeonCandle) { Items.RegisterScrap(candle, CandleSpawnWeight, (LevelTypes)(-1)); }
            if (EnableLighter) 
			{ 
				Items.RegisterScrap(lighter, LighterSpawnWeight, (LevelTypes)(-1));
				Items.RegisterScrap(bulletLighter, BulletLighterSpawnWeight, (LevelTypes)(-1));
			}
            if (EnableAbandonedFlashlight) { Items.RegisterScrap(abandonedflashlightitem, AbandonedBBFlashSpawnWeight, (LevelTypes)(-1)); }
			if (EnableIndustrialFlashlight) { Items.RegisterScrap(industrialflashlightitem, IndustrialFlashlightSpawnWeight, (LevelTypes)(-1)); }
			if (EnableProAbandonedFlashlight) { Items.RegisterScrap(proabaondedflashlightitem, AbandonedProFlashSpawnWeight, (LevelTypes)(-1)); }
			if (EnableAbandonedWalkie) { Items.RegisterScrap(abandonedWalkieItem, AbandonedWalkieSpawnWeight, (LevelTypes)(-1));  }
			if (EnableAbandonedTZP) { Items.RegisterScrap(abandonedTZPItem, AbandonedTZPSpawnWeight, (LevelTypes)(-1)); }
			





			lighter.toolTips = new string[] { "Light Flip Lighter : [LMB]" };
            bulletLighter.toolTips = new string[] { "Light Bullet Lighter : [LMB]" };
            candle.toolTips = new string[] { "Use item : [LMB]" };
            glowstick.toolTips = new string[] { "Drop Glowstick : [LMB]" };
            CandleStuff.minFail = 1;
            CandleStuff.maxFail = 4;

			//Figure out how to fix this at some point. Temp fix for now.
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "The Candle. A cheap but extremely unreliable method of lighting your way. The Company is not responsible for any fires.\n\n";
            /*/TerminalNode node2 = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "A stack of glowing sticks! The glowsticks do not produce much light, however come in VERY handy for keeping track of your path, they wont stay lit long so be quick! Some creatures may be interested in them, however.\n\n";/*/
            /*/TerminalNode node3 = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "An Emergency Signal Flare. Only to be used in the most dire of situations, this flare can be used to warn other employees or light the surrounding area in a bright red light. Caution advised when using. \n\n";/*/
			if (EnableShopCandle) { Items.RegisterShopItem(candle, null, null, node, 7); }
            if (EnableRandomGlowsticks) { Items.RegisterShopItem(glowstick, 15); }
			if (EnableFlare) { Items.RegisterShopItem(signalFlareItem, 100); }
			if (FlareCompatName) { signalFlareItem.itemName = "Military Flare"; }
			

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
		private void loadConfig()
		{
			FlareCompatName = Config.Bind<bool>("Extra", "EnableFlareCompatibilityName", false, "Should the Emergency Flare's name be changed to improve compatibility? [Default: false]").Value;

			EnableFlare = Config.Bind<bool>("General", "EnableFlare", true, "Should the Emergency Flare be available in the shop?").Value;
			EnableGlowsticks = Config.Bind<bool>("General", "EnableGlowsticks", true, "Should the Glowsticks be available in the shop?").Value;
			EnableLighter = Config.Bind<bool>("General", "EnableLighter", true, "Should the Lighters spawn in the dungeon?").Value;
			EnableShopCandle = Config.Bind<bool>("General", "EnableShopCandle", true, "Should the Antique Candles be available in the shop?").Value;
			EnableDungeonCandle = Config.Bind<bool>("General", "EnableDungeonCandle", true, "Should the Antique Candles spawn in the dungeon?").Value;
			EnableAbandonedFlashlight = Config.Bind<bool>("General", "EnableBBFlashlight", true, "Should the Abandoned Flashlights spawn in the dungeon?").Value;
			EnableRandomGlowsticks = Config.Bind<bool>("General", "EnableRandomGlowsticks", true, "Should the random glowsticks spawn in the dungeon?").Value;
			EnableIndustrialFlashlight = Config.Bind<bool>("General", "EnableIndustrialFlashlight", true, "Should the Industrial Flashlight spawn in the dungeon?").Value;
			EnableProAbandonedFlashlight = Config.Bind<bool>("General", "EnableProAbandonedFlashlight", true, "Should the Abandoned Pro-Flashlight spawn in the dungeon?").Value;
			EnableAbandonedWalkie = Config.Bind<bool>("General", "EnableAbandonedWalkieTalkie", true, "Should the Abandoned Walkie-Talkie spawn in the dungeon?").Value;
			EnableAbandonedTZP = Config.Bind<bool>("General", "EnableAbandonedTZP", true, "Should the Abandoned TZP-Inhaler spawn in the dungeon?").Value;

			CandleSpawnWeight = Config.Bind<int>("Spawn Weights", "CandleSpawnWeight", 10, "What should the spawn weight of the Antique Candle be? [Default: 10]").Value;
			LighterSpawnWeight = Config.Bind<int>("Spawn Weights", "LighterSpawnWeight", 20, "What should the spawn weight of the Lighter be? [Default: 20]").Value;
			BulletLighterSpawnWeight = Config.Bind<int>("Spawn Weights", "BulletLighterSpawnWeight", 2, "What should the spawn weight of the Bullet Lighter be? [Default: 2]").Value;
			AbandonedBBFlashSpawnWeight = Config.Bind<int>("Spawn Weights", "AbandonedBBFlashSpawnWeight", 30, "What should the spawn weight of the Abandoned Flashlight be? [Default: 30]").Value;
			AbandonedProFlashSpawnWeight = Config.Bind<int>("Spawn Weights", "AbandonedProFlashSpawnWeight", 20, "What should the spawn weight of the Abandoned Pro-Flashlight be? [Default: 20]").Value;
			IndustrialFlashlightSpawnWeight = Config.Bind<int>("Spawn Weights", "IndustrialFlashlightSpawnWeight", 5, "What should the spawn weight of the Industrial Flashlight be? [Default: 5]").Value;
			AbandonedWalkieSpawnWeight = Config.Bind<int>("Spawn Weights", "AbandonedWalkieSpawnWeight", 20, "What should the spawn weight of the Abandoned Walkie-Talkie be? [Default: 20]").Value;
			AbandonedTZPSpawnWeight = Config.Bind<int>("Spawn Weights", "AbandonedTZPSpawnWeight", 60, "What should the spawn weight of the Abandoned TZP-Inhaler be? [Default: 60]").Value;
			DroppedGlowstickSpawnWeight = Config.Bind<int>("Spawn Weights", "DropedGlowstickSpawnWeight", 10, "What should the spawn weight of the Dropped Glowstick be? [Default: 10]").Value;

		}
    }
}
