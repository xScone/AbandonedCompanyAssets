using AbandonedCompanyAssets.Behaviours;
using DigitalRuby.ThunderAndLightning;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AbandonedCompanyAssets.itemStuff
{
	internal class AbandonedFlashlightStuff : FlashlightItem
	{
		private int randomtrait;
		private bool traitGenerated;
		private float randomflickertime;
		private float flickertimer;
		private bool flickeron;
		private float flickertimebetween;
		private FlickeringLight playerlightsource;
		private AudioSource audiosource;
		public float defaultminintensity = 120f;
		public float defaultmaxintensity= 50f;
		public int defaultsmoothing = 20;
		private float randomchance;
		public bool isfaulty = true;
		public bool chargeonship = false;

		private float maintimer;
		private float revealtime;

		private float randomdroptime;
		private float droptimer;


		private float bulbfailtimer;
		private float randombulbfailtime;
		private int bulbfailures;
		private int maxbulbfailures;
		public static AudioClip bulbflicker = assetCall.bundle.LoadAsset<AudioClip>("Assets/Items/bbflashlight/flashlight flicker.wav");
		public static AudioClip bulbexplosion = assetCall.bundle.LoadAsset<AudioClip>("Assets/Items/bbflashlight/flashlight bulb explode.wav");

		 
		public override void LoadItemSaveData(int saveData)
		{
			base.LoadItemSaveData(saveData);
			short val1 = (short)(saveData >> 16);
			short val2 = (short)(saveData & 0xFFFF);
			//Plugin.ACALog.LogInfo("loaded battery: " + val2);
			this.randomtrait = (int)val1;
			this.maxbulbfailures = val2;
			saveData = 0;

		}
		public override int GetItemDataToSave()
		{
			base.GetItemDataToSave();
			Plugin.ACALog.LogInfo(this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge);
			short int1 = (short)randomtrait;
			short int2 = (short)maxbulbfailures;

			int combined = (int1 << 16) | (int2 & 0XFFFF);
			return combined;

		}

		public override void Start()
		{
			base.Start();
			if (UnityEngine.Random.Range(0f, 100f) <= 1)
			{
				this.gameObject.GetComponentInChildren<ScanNodeProperties>().subText = "(PlatformEffectfor2DEditor)";
			}
			audiosource = GetComponent<AudioSource>();
			isBeingUsed = false;
			if (maxbulbfailures == 0)
			{
				maxbulbfailures = UnityEngine.Random.Range(2, 5);
			}
			//convert the intensity values to be public variables so can be reused for pro flashlight.
			if (this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge == 0 && isfaulty)
			{
				this.GetComponent<FlashlightItem>().insertedBattery.charge = 1f * UnityEngine.Random.Range(0.2f, 0.95f);
			}
			else
			{
				this.GetComponent<FlashlightItem>().insertedBattery.charge = 1f;
			}
			
			revealtime = UnityEngine.Random.Range(60, 90);
			//make sure to save trait and then load trait here so that this will know it has a trait.
			if (randomtrait != 0)
			{
				traitGenerated = true;
			}
			if (this.gameObject.GetComponent<FlickeringLight>() == null) 
			{
				this.gameObject.AddComponent<FlickeringLight>();
				this.gameObject.GetComponent<FlickeringLight>().maxIntensity = defaultmaxintensity;
				this.gameObject.GetComponent<FlickeringLight>().minIntensity = defaultminintensity;
				this.gameObject.GetComponent<FlickeringLight>().smoothing = defaultsmoothing;
			}
		}
		public override void Update()
		{
			base.Update();
			if (chargeonship)
			{
				if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom && this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge < 100 && !isBeingUsed)
				{
					this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge += Time.deltaTime / 750;
				}
			}
			
			if (isfaulty) { 
				if (isHeld || isBeingUsed)
				{
					maintimer += Time.deltaTime;
				}
				if (maintimer >= revealtime && traitGenerated)
				{
					//flickering
					if (randomtrait == 1)
					{
						flickertimer += Time.deltaTime;
						flickeringLightServerRpc();
					}
					//faulty battery
					if (randomtrait == 2)
					{
						if (!isBeingUsed)
						{
							this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge -= Time.deltaTime / 350;
						}
					}
					//faulty fast drain battery
					if (randomtrait == 3)
					{
						if (isBeingUsed)
						{
							this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge -= Time.deltaTime / 200;
						}
					}
					if (randomtrait == 4)
					{
						droptimer += Time.deltaTime;
						if (isHeld && droptimer > randomdroptime && randomchance > 60)
						{
							GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GameNetworkManager.Instance.localPlayerController.transform.position, true);
							droptimer = 0;
						}
					}
					if (randomtrait == 5)
					{
						if (isBeingUsed)
						{
							bulbfailtimer += Time.deltaTime;
						}
						if (bulbfailtimer > randombulbfailtime && randomchance > 40)
						{
							bulbfailures += 1;
							Plugin.ACALog.LogInfo(bulbfailures);
							bulbfailtimer = 0;

							audiosource.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
							audiosource.PlayOneShot(bulbflicker);
							SwitchFlashlight(false);

						}
					}
				}
			}
		}
		public override void ChargeBatteries()
		{
			base.ChargeBatteries();
			bulbfailures = 0;
			bulbfailtimer = 0;
		}
		public override void ItemActivate(bool used, bool buttonDown = true)
		{
			base.ItemActivate(used, buttonDown);
			Plugin.ACALog.LogInfo(used);
			if (randomtrait == 5)
			{
				if (used && bulbfailures >= 2 && UnityEngine.Random.Range(0, 100) > 60)
				{
					audiosource.PlayOneShot(bulbexplosion);
					this.gameObject.GetComponent<FlashlightItem>().insertedBattery.charge = 0;
					dropFlashlightServerRpc();
					SwitchFlashlight(false);
				}
			}
			if (used)
			{
				randombulbfailtime = UnityEngine.Random.Range(45f, 120f);
			}
			if (usingPlayerHelmetLight)
			{
				playerlightsource.enabled = true;
			}
		}
		
		public override void GrabItem()
		{
			base.GrabItem();
			randomchance = UnityEngine.Random.Range(0, 100);
			addScriptServerRpc();
			if (!traitGenerated || randomtrait == 0)
			{
				randomtrait = UnityEngine.Random.Range(1, 7);
				traitGenerated = true;
				Plugin.ACALog.LogInfo(randomtrait);
			}
			if (randomtrait == 4)
			{
				randomdroptime = UnityEngine.Random.Range(120f, 240f);
			}
		}
		public override void PocketItem()
		{
			base.PocketItem();
			playerlightsource.enabled = true;
		}
		public override void DiscardItem()
		{
			base.DiscardItem();
			playerlightsource.enabled = false;
			previousPlayerHeldBy.helmetLight.intensity = 200;
			flickeron = false;
		}

		// flickering stuff
		[ServerRpc]
		public void dropFlashlightServerRpc()
		{
			dropFlashlightClientRpc();
		}
		[ClientRpc] 
		public void dropFlashlightClientRpc()
		{
			GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GameNetworkManager.Instance.localPlayerController.transform.position, true);
			droptimer = 0;
		}

		[ServerRpc]
		public void addScriptServerRpc()
		{
			addScriptClientRpc();
		}
		[ClientRpc] 
		public void addScriptClientRpc()
		{
			if (!playerHeldBy.helmetLight.gameObject.GetComponentInChildren<FlickeringLight>())
			{
				playerHeldBy.helmetLight.gameObject.AddComponent<FlickeringLight>();
			}
			playerlightsource = playerHeldBy.helmetLight.gameObject.GetComponentInChildren<FlickeringLight>();
			playerlightsource.maxIntensity = defaultmaxintensity;
			playerlightsource.minIntensity = defaultminintensity;
			playerlightsource.smoothing = defaultsmoothing;
			playerlightsource.enabled = false;
		}

		[ServerRpc(RequireOwnership = false)]
		private void flickeringLightServerRpc()
		{

			flickeringLightClientRpc();
		}
		[ClientRpc]
		private void flickeringLightClientRpc()
		{
			if (!flickeron)
			{
				this.flickeron = true;
				this.flickertimebetween = UnityEngine.Random.Range(0.5f, 2);
				this.randomflickertime = UnityEngine.Random.Range(5, 10);
			}
			else if (flickertimer >= randomflickertime)
			{
				this.gameObject.GetComponent<FlickeringLight>().maxIntensity = 40;
				this.gameObject.GetComponent<FlickeringLight>().minIntensity = 1;
				this.gameObject.GetComponent<FlickeringLight>().smoothing = 20;
				if (playerlightsource)
				{
					playerlightsource.maxIntensity = 40;
					playerlightsource.minIntensity = 1;
					playerlightsource.smoothing = 20;
				}
			}
			else
			{
				this.gameObject.GetComponent<FlickeringLight>().maxIntensity = 120;
				this.gameObject.GetComponent<FlickeringLight>().minIntensity = 50;
				this.gameObject.GetComponent<FlickeringLight>().smoothing = 40;
				if (playerlightsource)
				{
					playerlightsource.maxIntensity = defaultmaxintensity;
					playerlightsource.minIntensity = defaultminintensity;
					playerlightsource.smoothing = defaultsmoothing;
				}			
			}
			if (flickeron && flickertimer >= randomflickertime * flickertimebetween)
			{
				flickertimer = 0;
				flickeron = false;
			}
			
		}
	}
}