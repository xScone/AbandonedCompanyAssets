using DunGen.Graph;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using AbandonedCompanyAssets;
using System.Threading.Tasks;
using static UnityEngine.ParticleSystem;
using System.Collections;



namespace AbandonedCompanyAssets.itemStuff
{
    internal class GlowstickStuff : GrabbableObject
    {
        public PlayerControllerB currentPlayer;
        private int currentState;
        public Light lighting;
        public AudioClip glowstickSnap;
        public bool droppedStick;




        public override void Start()
        {
            base.Start();
            this.GetComponentInChildren<GrabbableObject>().insertedBattery.charge = 560;
            lighting = GetComponentInChildren<Light>();
            Plugin.ACALog.LogInfo("STARTASTATASETASTASTDGA,");
            base.EnablePhysics(true);
            base.EnableItemMeshes(true);
        }

        public override void Update()
        {
            base.Update();
            if (droppedStick)
            {
                this.GetComponentInChildren<GrabbableObject>().insertedBattery.charge -= Time.deltaTime % 60;
                if (this.GetComponentInChildren<GrabbableObject>().insertedBattery.charge < 3)
                {
                    glowstickDieServerRpc();
                    if (lighting.range > 0.05)
                    {
                        this.GetComponentInChildren<BoxCollider>().enabled = false;
                    }
                }
            }
            if (RoundManager.Instance.begunSpawningEnemies != true && droppedStick)
            {
				if (gameObject.GetComponentInChildren<NetworkObject>().IsOwner == true)
				{
					deleteSticksServerRpc();
				};
			}
        }
        public override void PocketItem()
        {
            base.PocketItem();
            isGlowstickLitServerRpc(false);
        }
        public override void EquipItem()
        {
            base.EquipItem();
            isGlowstickLitServerRpc(true);
        }

        public override int GetItemDataToSave()
        {
            base.GetItemDataToSave();
            Plugin.ACALog.LogInfo("Save Data:" + (currentState));
            return this.currentState;

        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            this.currentState = saveData;
            Plugin.ACALog.LogInfo("Load Data:" + (saveData));
            for (int i = 0; i < currentState; i++)
            {
                this.gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            Plugin.ACALog.LogInfo(currentState);
            Plugin.ACALog.LogInfo(Plugin.GlowstickDroppedItem.spawnPrefab.gameObject);
            Plugin.ACALog.LogInfo(OwnerClientId);
            this.currentState += 1;
            /*/if (currentState <= 3)
            {
                currentState += 1;
            }/*/




            Plugin.ACALog.LogInfo("AGAGAGDSAGHFASHPOKAGMDAOFPIGJKMPADF0OGL,");
			if (!droppedStick)
			{
				GlowstickSpawnServerRpc(gameObject.transform.position, GameNetworkManager.Instance.localPlayerController.transform.rotation);
			}

        }
		[ServerRpc]
		private void deleteSticksServerRpc()
		{
			this.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
			deleteClientRpc();
		}
		[ClientRpc] private void deleteClientRpc()
		{
			if (this.gameObject.GetComponentInChildren<NetworkObject>().IsOwner)
			{
				GameObject.Destroy(this.gameObject);
			}
		}

		[ServerRpc]
        private void GlowstickSpawnServerRpc(Vector3 position, Quaternion rotation)
        {
			GlowstickStackClientRpc();
			GameObject newObject2 = UnityEngine.Object.Instantiate(Plugin.GlowstickDroppedItem.spawnPrefab.gameObject, position, rotation, StartOfRound.Instance.propsContainer);
            newObject2.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
			if (this.currentState < 3)
            {
				newObject2.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
				newObject2.GetComponentInChildren<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                newObject2.GetComponentInChildren<AudioSource>().PlayOneShot(glowstickSnap);
                newObject2.GetComponentInChildren<GrabbableObject>().grabbable = false;
				
            }

        }
		[ServerRpc]
		private void isGlowstickLitServerRpc(bool lit)
        {
            isGlowstickLitClientRpc(lit);
        }
        [ClientRpc]
        private void isGlowstickLitClientRpc(bool lit)
        {
            if (lit)
            {
                this.lighting.enabled = true;
            }
            else
            {
                this.lighting.enabled = false;
            }
        }
		[ServerRpc]
		private void glowstickDieServerRpc()
        {
            glowstickDieClientRpc();
        }
        [ClientRpc]
        private void glowstickDieClientRpc()
        {
            this.lighting.intensity -= Time.deltaTime * 3;
            this.lighting.range -= Time.deltaTime * 3;
        }

        [ClientRpc] 
        private void GlowstickStackClientRpc() 
        {

			this.gameObject.transform.GetChild(currentState).gameObject.SetActive(false);
            if (this.currentState == 3)
            {
                playerHeldBy.DestroyItemInSlotAndSync(playerHeldBy.currentItemSlot);
            }
        }
    }
}
