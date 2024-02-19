using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.Rendering.HighDefinition.ProbeSettings;
using GameNetcodeStuff;
using static ES3;
using UnityEngine.UIElements;
using AbandonedCompanyAssets.Behaviours;

namespace AbandonedCompanyAssets.itemStuff
{
    internal class lighterStuff : GrabbableObject
    {
        private Light lighting;
        private ParticleSystem flame;
        private AudioSource audioSource;
        public AudioClip lighterFlick;
        public AudioClip burningFlame;
        public AudioClip lighterClose;
        public float fuel;
        private int currentState;
        private float timerDelay;
        private bool lighterLit;
        private float savedFuel;
        private bool lighterDead;
        private bool pocketingItem;

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            Plugin.ACALog.LogInfo("Load Data:" + (saveData));
            this.fuel = (float)saveData;
            if (saveData <= 0)
            {
                lighterDead = true;
            }
            else if (saveData > 0)
            {
                savedFuel = (float)saveData;
            }
        }
        public override int GetItemDataToSave()
        {
            base.GetItemDataToSave();
            Plugin.ACALog.LogInfo("Save Data:" + (int)Mathf.Round(fuel));
            return (int)Mathf.Round(fuel);
        }

        public override void Start()
        {
            base.Start();
            currentState = 0;
            lighting = GetComponentInChildren<Light>();
            flame = GetComponentInChildren<ParticleSystem>();
            audioSource = GetComponentInChildren<AudioSource>();
            this.GetComponentInChildren<GrabbableObject>().insertedBattery.charge = fuel;
            lighterServerRpc();

            if (savedFuel < 0)
            {
                fuel = savedFuel;
            }
            else if (!lighterDead)
            {
                fuel = UnityEngine.Random.Range(300, 1500);
            }

        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (currentState == 0)
            {
                currentState = 1;
            }
            else
            {
                currentState = 0;
            }
            timerDelay = 0;
            lighterServerRpc();
            Plugin.ACALog.LogInfo(currentState);


        }
        public override void PocketItem()
        {
            base.PocketItem();
            if (currentState == 0)
            {
                pocketingItem = true;
                timerDelay = 0;
                lighterServerRpc();
            }

        }
        public override void EquipItem()
        {
            base.EquipItem();
            if (pocketingItem)
            {
                pocketingItem = false;
                lighterServerRpc();
            }
        }
        public override void Update()
        {
            base.Update();
            var detectedObjects = Physics.OverlapSphere(GameNetworkManager.Instance.localPlayerController.transform.position, 2f, 1 << 21);
            if (currentState == 1 && lighterLit && !lighterDead)
            {
                foreach (var obj in detectedObjects)
                {
                    if (obj.gameObject.name == "WebContainer")
                    {
                        var web = obj.gameObject.GetComponent<SandSpiderWebTrap>();

                        if (IsHost)
                        {
                            web.mainScript.BreakWebClientRpc(web.transform.position, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                        }
                        else
                        {
                            web.mainScript.BreakWebServerRpc(web.trapID, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                        }
                        fireSpawnServerRpc(web.centerOfWeb.position);
                        fuel -= UnityEngine.Random.Range(30, 60);
                    }
                }
            }

            if (currentState == 1 && !pocketingItem)
            {
                timerDelay += Time.deltaTime;
                if (!lighterDead)
                {
                    fuel -= Time.deltaTime;
                }
            }
            if (timerDelay > 0.9 && currentState == 1)
            {
                lighterOnServerRpc();
                lighterLit = true;
            }
            if (fuel <= 0 && !lighterDead)
            {
                currentState = 0;
                lighterDeadServerRpc();
                lighterServerRpc();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        private void lighterOnServerRpc()
        {
            lighterOnClientRpc();
        }
        [ClientRpc]
        private void lighterOnClientRpc()
        {
            if (this.fuel > 5)
            {
                lighting.enabled = true;
                this.flame.Play();
                timerDelay = 0;
                audioSource.clip = burningFlame;
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.loop = true;
                audioSource.Play();
            }

        }
        [ServerRpc(RequireOwnership = false)]
        private void lighterServerRpc()
        {
            lighterClientRpc();
        }
        [ClientRpc]
        private void lighterClientRpc()
        {
            if (this.currentState == 0)
            {
                if (!pocketingItem)
                {
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                    this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
                this.flame.Clear();
                this.flame.Stop();
                audioSource.Stop();
                { audioSource.PlayOneShot(lighterClose); }
                lighting.enabled = false;
                lighterLit = false;
            }
            else
            {
                if (!pocketingItem)
                {
                    this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }
                audioSource.PlayOneShot(lighterFlick);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void lighterDeadServerRpc()
        {
            lighterDeadClientRpc();
        }
        [ClientRpc]
        private void lighterDeadClientRpc()
        {
            this.fuel = 0;
            this.lighterDead = true;
        }
        [ServerRpc(RequireOwnership = false)]
        private void fireSpawnServerRpc(Vector3 position)
        {
            fireSpawnClientRpc(position);
        }
        [ClientRpc]
        private void fireSpawnClientRpc(Vector3 position)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(Plugin.webBurnParticles.gameObject, position, new Quaternion(), StartOfRound.Instance.propsContainer);

        }
    }
}
