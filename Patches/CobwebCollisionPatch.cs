using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace AbandonedCompanyAssets.Patches
{
    /*/internal class CobwebCollisionPatch
    {
        public static void Init()
        {
            On.SandSpiderWebTrap.Awake += SandSpiderWebTrap_Awake;
        }

        private static void SandSpiderWebTrap_Awake(On.SandSpiderWebTrap.orig_Awake orig, SandSpiderWebTrap self)
        {
            orig(self);
            
            BoxCollider boxCollider = self.gameObject.AddComponent<BoxCollider>();
            //boxCollider.size = new UnityEngine.Vector3(10f, 10f, 10f);
            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
            boxCollider.includeLayers = 1 << 21;

        }
    }/*/
}
