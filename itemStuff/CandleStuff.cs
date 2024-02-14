using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using BepInEx.Logging;
using LethalLib;
using static UnityEngine.ParticleSystem.PlaybackState;
using GameNetcodeStuff;
using System.Runtime.CompilerServices;
using AbandonedCompanyAssets.Patches;
using UnityEngine.ProBuilder;


namespace AbandonedCompanyAssets.itemStuff
{
    internal class CandleStuff : GrabbableObject
    {

        private ParticleSystem particles;
        private Light lighting;
        private AudioSource audioSource;
        private PlayerControllerB player;
        public AudioClip flame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/CandleFlame.ogg");
        public AudioClip lightFlame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/LightCandle.ogg");
        public AudioClip blowFlame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/LightCandleBlow.ogg");

        public PlayerControllerB currentPlayer;
        private int failNumber = UnityEngine.Random.Range(minFail, maxFail);

        public int failCount;
        public int totalFailCount;
        public static int minFail;
        public static int maxFail;
        public bool currentlyLit;
        public static int candleAmount;

        private bool deadCandle;


        Queue<float> smoothQueue;
        float lastSum = 1;

        public override void Start()
        {
            base.Start();
            //savePatch.dataSave.failList.ForEach()
            candleAmount = candleAmount + 1;
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
        public override void EquipItem()
        {
            base.EquipItem();
            //lightCandle(false, false);

        }

        public override void GrabItem()
        {
            base.GrabItem();
            //lightCandle(true, false);
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
                lighting.enabled = false;
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.6f);
                audioSource.PlayOneShot(blowFlame);
            }
            else if (!currentlyLit && totalFailCount < failNumber && !freshCandle)
            {
                particles.Play();
                lighting.enabled = true;
                candleStart(true);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(lightFlame);
                currentlyLit = true;
                failCount = 0;
            }
            else if (totalFailCount < failNumber)
            {
                particles.Stop();
                lighting.enabled = false;
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(blowFlame);
                currentlyLit = false;
            }
            else
            {
                particles.Stop();
                lighting.enabled = false;
                candleStart(false);
                deadCandle = true;
            }

        }

    }
}
