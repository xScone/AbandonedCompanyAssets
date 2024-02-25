using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using static ES3;
using UnityEngine.UIElements;
using UnityEngine;
using GameNetcodeStuff;

namespace AbandonedCompanyAssets.itemStuff
{
    internal class SignalFlare : GrabbableObject
    {
        private Vector3 currentplayeposition;
        private bool flareused;
        private int monsteractivity;
        private ParticleSystem particles;
        public AudioClip thumpSound;
        private AudioSource audiosource;
		private bool deletestarted;
		private float time;

        public override void Start()
        {
            base.Start();
            particles = GetComponentInChildren<ParticleSystem>();
            audiosource = GetComponent<AudioSource>();
        }
		public override void Update()
		{
			base.Update();
			if (deletestarted)
			{
				time += Time.deltaTime;
			}
			if (time > 0.3	)
			{
				playerHeldBy.DestroyItemInSlotAndSync(playerHeldBy.currentItemSlot);
			}
		}

		public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
			audiosource.PlayOneShot(thumpSound);
			if ((!GameNetworkManager.Instance.localPlayerController.isInsideFactory) & (!GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)) 
            {
				currentplayeposition = GameNetworkManager.Instance.localPlayerController.transform.position;
                currentplayeposition.y += UnityEngine.Random.Range(80, 180);
                fireFlareServerRpc(currentplayeposition);
                flareused = true;
                monsteractivity = UnityEngine.Random.Range(0, 10);
                monsterRoundManagerServerRpc(monsteractivity);
            }
        }
        [ServerRpc]
        private void monsterRoundManagerServerRpc(int activity)
        {
			Plugin.ACALog.LogInfo(activity);
            if (activity == 8)
            {
                Plugin.ACALog.LogInfo("Default Power Level: " + RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount);
                RoundManager.Instance.currentLevel.maxEnemyPowerCount = RoundManager.Instance.currentLevel.maxEnemyPowerCount * 5;
                RoundManager.Instance.currentLevel.maxDaytimeEnemyPowerCount = RoundManager.Instance.currentLevel.maxDaytimeEnemyPowerCount * 5;
                RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount = RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount * 5;
                Plugin.ACALog.LogInfo("Edited Power Level: " + RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount);
				fireFlareClientRpc(2);
            }
            else if ((activity == 3) || (activity == 4))
            {
                Plugin.ACALog.LogInfo("Default Power Level: " + RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount);
                RoundManager.Instance.currentLevel.maxEnemyPowerCount = RoundManager.Instance.currentLevel.maxEnemyPowerCount * 2;
                RoundManager.Instance.currentLevel.maxDaytimeEnemyPowerCount = RoundManager.Instance.currentLevel.maxDaytimeEnemyPowerCount * 2;
                RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount = RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount * 2;
                Plugin.ACALog.LogInfo("Edited Power Level: " + RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount);
				fireFlareClientRpc(1);
            }
            else
            {
				fireFlareClientRpc(0);
            }
			
        }
        [ServerRpc]
        private void fireFlareServerRpc(Vector3 position)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(Plugin.signalFlareParticles.gameObject, position, new Quaternion(), StartOfRound.Instance.propsContainer);
			newObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }
        [ClientRpc]
        private void fireFlareClientRpc(int message)
        {
			particles.Play();
			
			if (message == 1)
			{
				if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory)
				{
					HUDManager.Instance.DisplayTip("WARNING!", "Increase in moon activity detected.", true);
				}
			}
			if (message == 2)
			{
				if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory)
				{
					HUDManager.Instance.DisplayTip("URGENT WARNING!", "MASSIVE INCREASE IN MOON ACTIVITY DETECTED, EVACTUATION ADVISED.", true);
				}
			}
			this.deletestarted = true;

		}
    }
}
