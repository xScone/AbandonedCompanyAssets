using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AbandonedCompanyAssets.Behaviours
{
    internal class DimThenKill : MonoBehaviour
    {
        private Light lightsource;
        public bool dimin;
        public float killafterseconds;
        private float time;
        public float dimrate;
        public float defaultrange;
        public float dimindelay;
        

        void Start ()
        {
            lightsource = GetComponentInChildren<Light>();
            Plugin.ACALog.LogInfo(defaultrange);
            
            if (dimrate == 0 ) 
            {
                dimrate = 0.005f;
            }
        }

        void Update ()
        {
            time += Time.deltaTime;
            if (time > dimindelay && time < killafterseconds)
            {
                if (lightsource.range < defaultrange)
                {
                    lightsource.range += dimrate;
                }
            }
            if (time > killafterseconds)
            {
                if (lightsource.range > 0)
                {
                    lightsource.range -= dimrate;
                }
                else if (lightsource.range <= 0)
                {
					if  (this.gameObject.GetComponentInChildren<NetworkObject>().IsOwner)
					{
						deleteThisServerRpc();
					}
                }
            }
        }
		[ServerRpc]
		private void deleteThisServerRpc()
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
