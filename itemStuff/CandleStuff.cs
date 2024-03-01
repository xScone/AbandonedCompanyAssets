using System.Collections.Generic;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;


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

        private int failCount;
        private int totalFailCount;
        public static int minFail;
        public static int maxFail;
        private bool currentlyLit;
        public float defaultLightIntensity;
        public float defaultLightRange;
        public float randomFailTimeMax;
        private float randomFailTime;


        public override void Start()
        {
            base.Start();
            failNumber = UnityEngine.Random.Range(minFail, maxFail);

            particles = GetComponentInChildren<ParticleSystem>();
            lighting = GetComponentInChildren<Light>();
            audioSource = GetComponentInChildren<AudioSource>();
            particles.Stop();
            particles.Clear();

            /*/if (!currentlyLit)
            {
                lightCandle(false, true);
                candleStart(false);
            }
            else
            {
                lightCandle(false, true);
                candleStart(true);
            }/*/


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
            saveData = 0;

        }

        public override void Update()
        {
            base.Update();
            if ((RoundManager.Instance.currentLevel.currentWeather == LevelWeatherType.Rainy) || (RoundManager.Instance.currentLevel.currentWeather == LevelWeatherType.Flooded) || (GameNetworkManager.Instance.localPlayerController.isUnderwater))
            {
                if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory && !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom && currentlyLit)
                {
                    totalFailCount += 1;
                    lightCandle(false, false);
                }
            }

            if (currentlyLit)
            {
                if (randomFailTimeMax == 0 && totalFailCount < failNumber)
                {
                    randomFailTimeMax = Random.Range(90f, 220f);
                }
                else if (totalFailCount < failNumber)
                {
                    randomFailTime += Time.deltaTime;
                }

                if (randomFailTime > randomFailTimeMax)
                {
                    if (Random.Range(0, 100) > 60)
                    {
                        particles.Stop();
                        particles.Clear();
                        candleStart(false);
                        lightCandle(false, false);
                        randomFailTime = 0;
                    }
                    else
                    {
                        randomFailTime = randomFailTime / 2;
                    }
                }
            }

            if (currentlyLit)
            {
                if (lighting.range < defaultLightRange)
                {
                    lighting.range += Time.deltaTime * 3;
                }
            }
            else
            {
                if (lighting.range > 0)
                {
                    lighting.range -= Time.deltaTime * 10;
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
            if (currentlyLit == true)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(blowFlame);
                particles.Stop();
                particles.Clear();
                candleStart(false);
                lightCandle(false, false);
            }
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if ((RoundManager.Instance.currentLevel.currentWeather == LevelWeatherType.Rainy) || (RoundManager.Instance.currentLevel.currentWeather == LevelWeatherType.Flooded))
            {
                if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory && !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                {
                    totalFailCount += 1;
                    audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.6f);
                    audioSource.PlayOneShot(blowFlame);

                }
                else
                {
                    lightCandle(false, false);
                }
            }
            Plugin.ACALog.LogInfo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            if (RoundManager.Instance.currentLevel.currentWeather != LevelWeatherType.Rainy)
            {
                lightCandle(false, false);
            }
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

            if (currentPlayer.insanityLevel >= 15 && failChance >= successChance * 3.5 && !grabbing && !currentlyLit && totalFailCount < failNumber && !freshCandle)
            {
                failCount = failCount + 1;
                totalFailCount = totalFailCount + 1;
                particles.Stop();
                particles.Clear();
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.6f);
                audioSource.PlayOneShot(blowFlame);
                candleIsLitServerRpc(false, false);
            }
            else if (!currentlyLit && totalFailCount < failNumber && !freshCandle)
            {
                particles.Play();
                candleStart(true);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(lightFlame);
                failCount = 0;
                candleIsLitServerRpc(true, false);
            }
            else if (totalFailCount < failNumber)
            {
                particles.Stop();
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(blowFlame);
                candleIsLitServerRpc(false, false);
            }
            else
            {
                particles.Stop();
                candleStart(false);
                candleIsLitServerRpc(false, true);
            }

        }
        [ServerRpc]
        private void candleIsLitServerRpc(bool lit, bool deadCandle)
        {
            candleIsLitClientRpc(lit, !deadCandle);
            if (deadCandle)
            {
                candleIsLitClientRpc(false, true);
            }
        }
        [ClientRpc]
        private void candleIsLitClientRpc(bool lit, bool deadCandle)
        {
            if (lit)
            {
                this.currentlyLit = true;
            }
            else
            {
                this.currentlyLit = false;
            }
        }
    }
}
