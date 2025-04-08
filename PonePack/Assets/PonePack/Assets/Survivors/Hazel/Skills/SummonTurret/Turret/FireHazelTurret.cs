using EntityStates;
using RoR2;
using RoR2.Networking;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class FireHazelTurret : GenericBulletBaseState
    {
        //public static GameObject effectPrefab;
        //public static string attackSoundString;
        //private static int FireGaussStateHash = Animator.StringToHash("FireGauss");
        //private static int FireGaussParamHash = Animator.StringToHash("FireGauss.playbackRate");
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}