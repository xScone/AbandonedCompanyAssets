using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AbandonedCompanyAssets.Behaviours
{
    internal class AutoPlaySounds : MonoBehaviour
    {
        private AudioSource audiosource;
        public AudioClip startaudio;
        public AudioClip audio1;
        public AudioClip audioloop;
        public float audioclipdelay;
        public float audioloopdelay;
        private bool sound1;
        private bool sound2;
        private float time;

        void Start()
        {
            audiosource = GetComponentInChildren<AudioSource>();
            audiosource.PlayOneShot(startaudio);
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time > audioclipdelay && !sound1)
            {
                Plugin.ACALog.LogInfo("Playing Sound 1");
                audiosource.PlayOneShot(audio1);
                sound1 = true;
            }
            
            if (time > audioloopdelay && !sound2)
            {
                audiosource.clip = audioloop;
                audiosource.loop = true;
                audiosource.Play();
                sound2 = true;
            }
        }
    }
}
