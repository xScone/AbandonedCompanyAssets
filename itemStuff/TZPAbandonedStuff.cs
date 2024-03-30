using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AbandonedCompanyAssets.itemStuff
{
	internal class TZPAbandonedStuff : TetraChemicalItem
	{
		private float damagetimer;
		private float damagetime = 1;
		private int damageAmount;

		private int playerhealth;
		public override void Start()
		{
			base.Start();
			for (int i = 0; i < GameNetworkManager.Instance.connectedPlayers; i++)
			{
				playerhealth = StartOfRound.Instance.allPlayerScripts[i].health;
			}
		}

		public override void Update()
		{
			base.Update();
			if (isBeingUsed && emittingGas)
			{
				damagetimer += Time.deltaTime;
			}
			if (emittingGas)
			{
				previousPlayerHeldBy.drunknessInertia = Mathf.Clamp(previousPlayerHeldBy.drunknessInertia + Time.deltaTime / 1f * previousPlayerHeldBy.drunknessSpeed, 0.1f, 10f);
			}
			if (damagetimer >= damagetime)
			{
				if (IsOwner)
				{
					GameNetworkManager.Instance.localPlayerController.DamagePlayer(damageAmount, false);
					damageAmount = UnityEngine.Random.Range(0, playerhealth);
					damagetimer = 0;
				}
				
			}
		}
		public override void EquipItem()
		{
			base.EquipItem();
			damagetime = UnityEngine.Random.Range(2, 10);
			damageAmount = UnityEngine.Random.Range(0, playerhealth);
		}
	}
}
