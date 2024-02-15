using System.Collections.Generic;
using UnityEngine;
using GameNetcodeStuff;


namespace AbandonedCompanyAssets.itemStuff
{
    internal class CandleStuff : GrabbableObject
    {

        private ParticleSystem particles;
        private Light lighting;
        private AudioSource audioSource;
        private PlayerControllerB player;
        public AudioClip flame;
        public AudioClip lightFlame;
        public AudioClip blowFlame;

        public PlayerControllerB currentPlayer;
        private int failNumber = UnityEngine.Random.Range(minFail, maxFail);

        public int failCount;
        public int totalFailCount;
        public static int minFail;
        public static int maxFail;
        public bool currentlyLit;
        public float defaultLightIntensity;
        public float defaultLightRange;


        private bool candleDead;


        public override void Start()
        {
            base.Start();
            failNumber = UnityEngine.Random.Range(minFail, maxFail);

            particles = GetComponentInChildren<ParticleSystem>();
            lighting = GetComponentInChildren<Light>();
            audioSource = GetComponentInChildren<AudioSource>();

            if (!currentlyLit)
            {
                lightCandle(false, true);
                candleStart(false);
            }
            else
            {
                lightCandle(false, true);
                candleStart(true);
            }


            if (particles == null)
            {
                Plugin.ACALog.LogDebug("WHERE THE FUCK ARE MY PARTICLES");
            }
            if (lighting == null)
            {
                Plugin.ACALog.LogDebug("WHERE THE FUCK ARE MY PARTICLES");
            }
        }
        public override int GetItemDataToSave()
        {
            base.GetItemDataToSave();
            short int1 = (short) failNumber;
            short int2 = (short) totalFailCount;
            
            int combined = (int1 << 16) | (int2 & 0XFFFF);
            return combined;
            
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            short val1 = (short) (saveData >> 16);
            short val2 = (short) (saveData & 0xFFFF);
            this.failNumber = val1;
            this.totalFailCount = val2;

        }

        public override void Update()
        {
            base.Update();
            if (currentlyLit)
            {
                if (lighting.intensity < defaultLightIntensity && lighting.range < defaultLightRange)
                {
                    lighting.intensity += Time.deltaTime * 5;
                    lighting.range += Time.deltaTime * 5;
                }
            }
            else
            {
                if (lighting.intensity > 0 && lighting.range > 0)
                {
                    lighting.intensity -= Time.deltaTime * 5;
                    lighting.range -= Time.deltaTime * 5;
                }
            }

        }
        public override void DiscardItem()
        {
            base.DiscardItem();
            if (currentlyLit)
            {
                particles.Play();
                candleStart(true);
            }
            else
            {
                particles.Stop();
                particles.Clear();
                candleStart(false);
            }
        }

        public override void PocketItem()
        {
            base.PocketItem();
            particles.Stop();
            particles.Clear();
            lighting.enabled = false;
            candleStart(false);
            if (currentlyLit == true)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(blowFlame);
                currentlyLit = false;
            }
            else
            {
                return;
            }
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            Plugin.ACALog.LogInfo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            lightCandle(false, false);
        }

        private void candleStart(bool startCandle)
        {
            if (startCandle)
            {
                audioSource.clip = flame;
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }

        public void lightCandle(bool grabbing, bool freshCandle)
        {
            currentPlayer = GameNetworkManager.Instance.localPlayerController;
            int sanityReduction = (int)currentPlayer.insanityLevel / 2;
            int failChance = UnityEngine.Random.Range(1, 100);
            int successChance = UnityEngine.Random.Range(1, 100 - sanityReduction);

            if (currentPlayer.insanityLevel >= 15 && failChance >= successChance * 3.5 && !grabbing && !currentlyLit && failCount < 2 && totalFailCount < failNumber && !freshCandle)
            {
                failCount = failCount + 1;
                totalFailCount = totalFailCount + 1;
                particles.Stop();
                particles.Clear();
                currentlyLit = false;
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.6f);
                audioSource.PlayOneShot(blowFlame);
            }
            else if (!currentlyLit && totalFailCount < failNumber && !freshCandle)
            {
                particles.Play();
                candleStart(true);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(lightFlame);
                currentlyLit = true;
                failCount = 0;
            }
            else if (totalFailCount < failNumber)
            {
                particles.Stop();
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(blowFlame);
                currentlyLit = false;
            }
            else
            {
                particles.Stop();
                candleStart(false);
            }

        }

    }
}
