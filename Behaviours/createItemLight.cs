using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using BepInEx.Logging;
using LethalLib;
using static UnityEngine.ParticleSystem.PlaybackState;
using GameNetcodeStuff;
using System.Runtime.CompilerServices;
using Unity.Netcode;


namespace AbandonedCompanyAssets.Behaviours
{
    internal class createItemLight : PhysicsProp
    {

        private ParticleSystem particles;
        private Light lighting;
        private AudioSource audioSource;
        private PlayerControllerB player;
        public AudioClip flame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/CandleFlame.ogg");
        public AudioClip lightFlame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/LightCandle.ogg");
        public AudioClip blowFlame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/LightCandleBlow.ogg");

        //stuff for light flicker
        public float minIntensity = 0f;
        public float maxIntensity = 400f;
        public int smoothing = 35;
        Queue<float> smoothQueue;
        float lastSum = 0;
        bool currentlyLit;

        public PlayerControllerB currentPlayer;

        public int failCount;

        public void Reset()
        {
            smoothQueue.Clear();
            lastSum = 0;
        }

        public override void Start()
        {
            base.Start();


            particles = GetComponentInChildren<ParticleSystem>();
            lighting = GetComponentInChildren<Light>();
            audioSource = GetComponentInChildren<AudioSource>();



            var myLogger = new ManualLogSource("ACALogger");
            BepInEx.Logging.Logger.Sources.Add(myLogger);
            if (particles == null)
            {
                myLogger.LogDebug("WHERE THE FUCK ARE MY PARTICLES");
            }
            if (lighting == null)
            {
                myLogger.LogDebug("WHERE THE FUCK ARE MY LIGHTINGS");
            }
        }
        public override void EquipItem()
        {
            base.EquipItem();
            lightCandle(false);

        }

        public override void GrabItem()
        {
            base.GrabItem();
            lightCandle(true);
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

        public void lightCandle(bool grabbing)
        {
            currentPlayer = GameNetworkManager.Instance.localPlayerController;
            int sanityReduction = (int)currentPlayer.insanityLevel / 2;


            int failChance = UnityEngine.Random.Range(1, 100);
            int successChance = UnityEngine.Random.Range(1, (100 - sanityReduction));
            if (grabbing != true && !currentlyLit)
            
            if (currentPlayer.insanityLevel >= 15 && (float)failChance >= ((float)successChance * 3.5) && (!grabbing & !currentlyLit) && failCount < 2)
            {
                failCount = failCount + 1;
                particles.Stop();
                particles.Clear();
                currentlyLit = false;
                lighting.enabled = false;
                candleStart(false);
                audioSource.pitch = UnityEngine.Random.Range(0.4f, 0.6f);
                audioSource.PlayOneShot(blowFlame);
            }
            else if (!currentlyLit)
            {
                particles.Play();
                lighting.enabled = true;
                candleStart(true);
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(lightFlame);
                currentlyLit = true;
                failCount = 0;
            }
        }

        
    }
}
