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



namespace AbandonedCompanyAssets.itemStuff
{
    internal class GlowstickStuff : GrabbableObject
    {
        public PlayerControllerB currentPlayer;
        private int currentState;
        public static GameObject spawnedGlowstick;
        public Light lighting;
        public AudioClip glowstickSnap = assetCall.bundle.LoadAsset<AudioClip>("Assets/Working Shit/glowstickSnap 1.wav");




        public override void Start()
        {
            base.Start();
            lighting = GetComponentInChildren<Light>();
            Plugin.ACALog.LogInfo("STARTASTATASETASTASTDGA,");
            base.EnablePhysics(true);
            base.EnableItemMeshes(true);
        }
        public override void PocketItem()
        {
            base.PocketItem();
            lighting.enabled = false;
        }
        public override void EquipItem()
        {
            base.EquipItem();
            lighting.enabled = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            Plugin.ACALog.LogInfo(currentState);
            Plugin.ACALog.LogInfo(spawnedGlowstick);
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
            Plugin.ACALog.LogInfo(spawnedGlowstick);
            GameObject newObject = UnityEngine.Object.Instantiate(spawnedGlowstick, position, rotation, StartOfRound.Instance.propsContainer);
            newObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            //newObject.GetComponent<ScanNodeProperties>().scrapValue = 0;
            if (this.currentState < 3)
            {
                newObject.GetComponentInChildren<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                newObject.GetComponentInChildren<AudioSource>().PlayOneShot(glowstickSnap);
                currentState += 1;
            }
            GlowstickStackClientRpc();

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
