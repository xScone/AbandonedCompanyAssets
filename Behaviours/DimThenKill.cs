using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AbandonedCompanyAssets.Behaviours
{
    internal class DimThenKill : MonoBehaviour
    {
        private Light lightSource;

        void Start ()
        {
            lightSource = GetComponent<Light>();
        }

        void Update ()
        {
            if (lightSource.range > 0)
            {
                lightSource.range -= 0.005f;
            }
            else if (lightSource.range <= 0) 
            {
                GameObject.Destroy(this);
            }
        }
    }
}
