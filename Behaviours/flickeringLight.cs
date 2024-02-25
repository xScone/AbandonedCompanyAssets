using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using BepInEx.Logging;
using BepInEx;
using static UnityEngine.Rendering.HighDefinition.ProbeSettings;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace AbandonedCompanyAssets.Behaviours
{
    internal class FlickeringLight : MonoBehaviour
    {
        private Light lightSource;
        public float minIntensity = 100f;
        public float maxIntensity = 200f;
        public int smoothing = 35;


        Queue<float> smoothQueue;
        float lastSum = 1;

        public void Reset()
        {
            smoothQueue.Clear();
            lastSum = 0;
            
        }

        public void Start()
        {
            smoothQueue = new Queue<float>(smoothing);
            // External or internal light?
            if (lightSource == null)
            {
                lightSource = GetComponentInChildren<Light>();
            }
        }

        public void Update()
        {
            if (lightSource == null)
                return;

            // pop off an item if too big
            while (smoothQueue.Count >= smoothing)
            {
                lastSum -= smoothQueue.Dequeue();
            }

            // Generate random new item, calculate new average
            float newVal = UnityEngine.Random.Range(minIntensity, maxIntensity);
            smoothQueue.Enqueue(newVal);
            lastSum += newVal;

            // Calculate new smoothed average
            lightSource.intensity = lastSum / (float)smoothQueue.Count;
        }

    }
}
