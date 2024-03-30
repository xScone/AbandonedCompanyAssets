using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace AbandonedCompanyAssets.itemStuff
{
	internal class AbandonedWalkieStuff : WalkieTalkie
	{
		private int randomtrait;
		private bool traitGenerated;
		public bool isfaulty = true;
		private float powertimer;
		private float randompowerofftime;
		private float randomgarbletime;
		private float stoprandomgarble;
		private bool gentimers;

		private float maintimer;
		private float revealtime;
		private float garbletimer;
		public override void LoadItemSaveData(int saveData)
		{
			base.LoadItemSaveData(saveData);
			Plugin.ACALog.LogInfo("Load Data:" + (saveData));
			this.randomtrait = (int)saveData;
		}
		public override int GetItemDataToSave()
		{
			base.GetItemDataToSave();
			Plugin.ACALog.LogInfo("Save Data:" + randomtrait);
			return randomtrait;
		}
		public override void Start()
		{
			base.Start();
			revealtime = UnityEngine.Random.Range(60, 90);
			randompowerofftime = UnityEngine.Random.Range(180f, 360f);
			randomgarbletime = UnityEngine.Random.Range(60f, 180f);
			stoprandomgarble = UnityEngine.Random.Range(10f, 30f);
			this.gameObject.GetComponent<WalkieTalkie>().insertedBattery.charge = UnityEngine.Random.Range(0.3f, 0.8f);
		}

		public override void Update()
		{
			base.Update();
			if (isfaulty)
			{
				if (isHeld || isBeingUsed)
				{
					maintimer += Time.deltaTime;
					if (randomtrait == 3 || randomtrait == 5)
					{
						powertimer += Time.deltaTime;
						
					}
				}
				if (maintimer >= revealtime && traitGenerated)
				{
					if (randomtrait == 1)
					{
						if (this.gameObject.GetComponentByName("Target").GetComponent<AudioDistortionFilter>() != null)
						{
							this.gameObject.GetComponentByName("Target").GetComponent<AudioDistortionFilter>().distortionLevel = 0.7f;
						}
						else
						{
							if (IsOwner)
							{
								generateTraitServerRpc();
							}
						}
					}
					if (randomtrait == 2)
					{
						if (isBeingUsed)
						{
							this.gameObject.GetComponent<WalkieTalkie>().insertedBattery.charge -= Time.deltaTime / 200;
						}
					}
					if (randomtrait == 4)
					{
						if (powertimer >= randompowerofftime)
						{
							SwitchWalkieTalkieOn(false);
							powertimer = 0;
						}
					}
					if (randomtrait == 5)
					{
						if (powertimer >= randomgarbletime)
						{
							playingGarbledVoice = true;
							garbletimer += Time.deltaTime;
						}
                        if (garbletimer >= stoprandomgarble)
						{
							playingGarbledVoice = false;
							powertimer = 0;
							garbletimer = 0;
						}
					}
					if (randomtrait == 6)
					{
						if (!isBeingUsed && this.gameObject.GetComponent<WalkieTalkie>().insertedBattery.charge > 0)
						{
							this.gameObject.GetComponent<WalkieTalkie>().insertedBattery.charge -= Time.deltaTime / 350;
						}
					}
				}
			}
		}

		public override void GrabItem()
		{
			base.GrabItem();
			if (!traitGenerated || randomtrait == 0)
			{
				if (IsOwner)
				{
					generateTraitServerRpc();
				}
			}
			
		}
		public override void ChargeBatteries()
		{
			base.ChargeBatteries();
			if (randomtrait == 3)
			{
				this.gameObject.GetComponent<WalkieTalkie>().insertedBattery.charge = 0.5f;
			}
		}
		public override void ItemActivate(bool used, bool buttonDown = true)
		{
			base.ItemActivate(used, buttonDown);
		}

		[ServerRpc]
		public void generateTraitServerRpc()
		{
			generateTraitClientRpc();
		}
		[ClientRpc]
		public void generateTraitClientRpc()
		{
			randomtrait = UnityEngine.Random.Range(1, 7);
			traitGenerated = true;
			Plugin.ACALog.LogInfo(randomtrait);
		}
	}
}
