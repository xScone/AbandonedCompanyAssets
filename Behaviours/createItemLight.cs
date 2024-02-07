using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using BepInEx.Logging;
using static UnityEngine.ParticleSystem.PlaybackState;


namespace AbandonedCompanyAssets.Behaviours
{
    internal class createItemLight : PhysicsProp
    {
        
        private ParticleSystem particles;
        private Light lighting;
        private AudioSource audioSource;
        public AudioClip flame = assetCall.bundle.LoadAsset<AudioClip>("Assets/Ripped Assets/AudioClip/CandleFlame.ogg");
        //stuff for light flicker
        public float minIntensity = 0f;
        public float maxIntensity = 400f;
        public int smoothing = 35;

        Queue<float> smoothQueue;
        float lastSum = 0;

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
            if (particles == null )
            {
                myLogger.LogDebug("WHERE THE FUCK ARE MY PARTICLES");
            }
            if (lighting == null )
            {
                myLogger.LogDebug("WHERE THE FUCK ARE MY LIGHTINGS");
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            particles.Play();
            lighting.enabled = true;
            candleStart(true);
        }
        public override void DiscardItem()
        {
            base.DiscardItem();
            base.EquipItem();
            particles.Play();
            lighting.enabled = true;
            candleStart(true);
        }

        public override void PocketItem()
        {
            base.PocketItem();
            particles.Stop();
            particles.Clear();
            lighting.enabled = false;
            candleStart(false);
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
    }
}
