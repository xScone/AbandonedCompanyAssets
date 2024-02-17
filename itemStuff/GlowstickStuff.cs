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
            /*/if (currentState <= 3)
            {
                currentState += 1;
            }/*/




            Plugin.ACALog.LogInfo("AGAGAGDSAGHFASHPOKAGMDAOFPIGJKMPADF0OGL,");
            GlowstickSpawnServerRpc(gameObject.transform.position, gameObject.transform.rotation);

        }


        [ServerRpc]
        private void GlowstickSpawnServerRpc(Vector3 position, Quaternion rotation)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(Plugin.GlowstickDroppedItem.spawnPrefab.gameObject, position, rotation, StartOfRound.Instance.propsContainer);
            newObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            //newObject.GetComponent<ScanNodeProperties>().scrapValue = 0;
            if (this.currentState < 3)
            {
                newObject.GetComponentInChildren<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                newObject.GetComponentInChildren<AudioSource>().PlayOneShot(glowstickSnap);
                newObject.GetComponentInChildren<GrabbableObject>().grabbable = false;
                currentState += 1;
            }
            GlowstickStackClientRpc();

        }
        [ServerRpc]
        private void isGlowstickLitServerRpc(bool lit)
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
