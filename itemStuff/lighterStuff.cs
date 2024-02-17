using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.Rendering.HighDefinition.ProbeSettings;

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
            lighting = GetComponentInChildren<Light>();
            flame = GetComponentInChildren<ParticleSystem>();
            audioSource = GetComponentInChildren<AudioSource>();
            currentState = 0;
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
            lighterLit = true;
            timerDelay = 0;
            lighterServerRpc();


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
            if (currentState == 0 && !pocketingItem)
            {
                timerDelay += Time.deltaTime;
                if (!lighterDead)
                {
                    fuel -= Time.deltaTime;
                }
            }
            if (timerDelay > 0.9 && currentState == 0)
            {
                lighterOnServerRpc();
            }
            if (fuel <= 0 && !lighterDead)
            {
                lighterDeadServerRpc();
                lighterClientRpc();

            }

        }


        [ServerRpc]
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
        [ServerRpc]
        private void lighterOffServerRpc()
        {
            lighterOffClientRpc();
        }
        [ClientRpc]
        private void lighterOffClientRpc()
        {
            lighting.enabled = false;
            this.flame.Clear();
            this.flame.Stop();
            this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            this.audioSource.PlayOneShot(lighterClose);
            audioSource.loop = false;
            lighterLit = false;
            currentState = 1;
        }
        [ServerRpc]
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
                currentState = 1;

            }
            else
            {
                if (!pocketingItem)
                {
                    this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }

                audioSource.PlayOneShot(lighterFlick);
                currentState = 0;
            }
        }

        [ServerRpc]
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
    }
}
