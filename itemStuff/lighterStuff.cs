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
        private AudioSource audiosource;
        public AudioClip lighterflick;
        public AudioClip burningflame;
        public AudioClip lighterclose;
        public float fuel;
        private int currentstate;
        private float timerdelay;
        private bool lighterlit;
        private float savedfuel;
        private bool lighterdead;
        private bool pocketingitem;
        public bool usesfueltowebburn;
        public bool getsextrafuel;

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            Plugin.ACALog.LogInfo("Load Data:" + (saveData));
            this.fuel = (float)saveData;
            if (saveData <= 0)
            {
                lighterdead = true;
            }
            else if (saveData > 0)
            {
                savedfuel = (float)saveData;
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
            currentstate = 0;
            lighting = GetComponentInChildren<Light>();
            flame = GetComponentInChildren<ParticleSystem>();
            audiosource = GetComponentInChildren<AudioSource>();
            this.GetComponentInChildren<GrabbableObject>().insertedBattery.charge = fuel;
            if (savedfuel < 0)
            {
                fuel = savedfuel;
            }
            else if (!lighterdead)
            {
                if (getsextrafuel)
                {
                    fuel = UnityEngine.Random.Range(500, 2000);
                }
                else
                {
                    fuel = UnityEngine.Random.Range(300, 1500);
                }
            }
            lighterServerRpc();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (currentstate == 0)
            {
                currentstate = 1;
            }
            else
            {
                currentstate = 0;
            }
            timerdelay = 0;
            lighterServerRpc();
            Plugin.ACALog.LogInfo(currentstate);


        }
        public override void PocketItem()
        {
            base.PocketItem();
            if (currentstate == 1)
            {
                pocketingitem = true;
                timerdelay = 0;
                currentstate = 0;
				lighterlit = false;
				lighterServerRpc();
            }

        }
        public override void EquipItem()
        {
            base.EquipItem();
            if (pocketingitem)
            {
                currentstate = 1;
                pocketingitem = false;
                lighterServerRpc();
            }
        }
        public override void Update()
        {
            base.Update();
            var detectedObjects = Physics.OverlapSphere(GameNetworkManager.Instance.localPlayerController.transform.position, 2f, 1 << 21);
            if (currentstate == 1 && lighterlit && !lighterdead)
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
                        if (usesfueltowebburn)
                        {
                            fuel -= UnityEngine.Random.Range(30, 60);
                        }
                    }
                }
            }

            if (currentstate == 1 && !pocketingitem)
            {
                timerdelay += Time.deltaTime;
                if (!lighterdead)
                {
                    fuel -= Time.deltaTime;
                }
            }
            if (timerdelay > 0.9 && currentstate == 1)
            {
                lighterOnServerRpc();
                lighterlit = true;
            }
            if (fuel <= 0 && !lighterdead)
            {
                currentstate = 0;
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
                timerdelay = 0;
                audiosource.clip = burningflame;
                audiosource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audiosource.loop = true;
                audiosource.Play();
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
            if (this.currentstate == 0)
            {
                if (!pocketingitem)
                {
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                    this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
                this.flame.Clear();
                this.flame.Stop();
                audiosource.Stop();
                { audiosource.PlayOneShot(lighterclose); }
                lighting.enabled = false;
                lighterlit = false;
            }
            else
            {
                if (!pocketingitem)
                {
                    this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }
                audiosource.PlayOneShot(lighterflick);
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
            this.lighterdead = true;
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
