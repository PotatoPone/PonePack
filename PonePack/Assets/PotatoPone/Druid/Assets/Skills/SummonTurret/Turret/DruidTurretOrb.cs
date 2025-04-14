using RoR2.Orbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Druid
{
    public class DruidTurretOrb : GenericDamageOrb
    {
        public override void Begin()
        {
            this.speed = 75f;
            base.Begin();
        }

        public override GameObject GetOrbEffect()
        {
            return Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MissileVoid/MissileVoidOrbEffect.prefab").WaitForCompletion();
        }
    }
}
