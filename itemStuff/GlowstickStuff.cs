using DunGen.Graph;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;



namespace AbandonedCompanyAssets.itemStuff
{
    internal class GlowstickStuff : GrabbableObject
    {
        public PlayerControllerB currentPlayer;
        private int currentState;
        public static GameObject clonePrefab;




        public override void Start()
        {
            base.Start();
            Plugin.ACALog.LogInfo("STARTASTATASETASTASTDGA,");
            base.EnablePhysics(true);
            base.EnableItemMeshes(true);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (currentState < 3)
            {
                currentState += 1;
            }
            Plugin.ACALog.LogInfo(clonePrefab);
            Plugin.ACALog.LogInfo(OwnerClientId);
            


            Plugin.ACALog.LogInfo("AGAGAGDSAGHFASHPOKAGMDAOFPIGJKMPADF0OGL,");
            GlowstickSpawnServerRpc(gameObject.transform.position, gameObject.transform.rotation);
        }
        [ServerRpc]
        private void GlowstickSpawnServerRpc(Vector3 position, Quaternion rotation)
        {
            Plugin.ACALog.LogInfo(clonePrefab);
            GameObject newObject = UnityEngine.Object.Instantiate(clonePrefab, position, rotation, StartOfRound.Instance.propsContainer);
            newObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }
    }
}
