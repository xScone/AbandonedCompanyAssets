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
		private bool hasbeengrabbed;
		private float randomflickertime;
		private float flickertimer;
		private bool flickeron;
		private float flickertimebetween;
		private FlickeringLight playerlightsource;

		private float maintimer;
		private float revealtime;

		public override void Start()
		{
			base.Start();
			revealtime = UnityEngine.Random.Range(30, 90);
			//make sure to save trait and then load trait here so that this will know it has a trait.
			if (randomtrait != 0)
			{
				traitGenerated = true;
			}
			//REMEMBER TO REMOVE THIS LATER
			randomtrait = 1;
			traitGenerated = true;
			//#############
			this.gameObject.AddComponent<FlickeringLight>();

			this.gameObject.GetComponent<FlickeringLight>().maxIntensity = 120;
			this.gameObject.GetComponent<FlickeringLight>().minIntensity = 50;
			this.gameObject.GetComponent<FlickeringLight>().smoothing = 40;

		}
		public override void Update()
		{
			base.Update();
			maintimer += Time.deltaTime;
			if (maintimer >= revealtime || traitGenerated)
			{
				if (randomtrait == 1)
				{
					flickeringLightServerRpc();
					flickertimer += Time.deltaTime;
				}
				//flickering is working, move onto next effects when coming back.
			}
		}
		public override void ItemActivate(bool used, bool buttonDown = true)
		{
			base.ItemActivate(used, buttonDown);
			if (usingPlayerHelmetLight)
			{
				playerlightsource.enabled = true;
			}
		}

		public override void GrabItem()
		{
			base.GrabItem();
			addScriptServerRpc();
			if (!traitGenerated || randomtrait == 0)
			{
				randomtrait = UnityEngine.Random.Range(1, 5);
				traitGenerated = true;
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
			playerlightsource.maxIntensity = 120;
			playerlightsource.minIntensity = 50;
			playerlightsource.smoothing = 40;
			playerlightsource.enabled = false;
		}

		[ServerRpc(RequireOwnership = false)]
		private void flickeringLightServerRpc()
		{
			if (!flickeron)
			{
				flickertimebetween = UnityEngine.Random.Range(0.5f, 2);
				randomflickertime = UnityEngine.Random.Range(5, 10);
				flickeron = true;
			}
			flickeringLightClientRpc();
		}
		[ClientRpc]
		private void flickeringLightClientRpc()
		{
			if (flickertimer >= randomflickertime)
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
					playerlightsource.maxIntensity = 120;
					playerlightsource.minIntensity = 50;
					playerlightsource.smoothing = 40;
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