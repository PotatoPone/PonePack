using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class DeathState : GenericCharacterDeath
    {

        [SerializeField]
        public GameObject initialExplosion;
        [SerializeField]
        public GameObject deathExplosion;
        private float deathDuration;

        public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
        {
            Animator modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                int layerIndex = modelAnimator.GetLayerIndex("Body");
                modelAnimator.PlayInFixedTime("Death", layerIndex);
                modelAnimator.Update(0f);
                this.deathDuration = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).length;
                if (this.initialExplosion)
                {
                    UnityEngine.Object.Instantiate<GameObject>(this.initialExplosion, base.transform.position, base.transform.rotation, base.transform);
                }
            }
        }

        public override bool shouldAutoDestroy
        {
            get
            {
                return true; //test
                //return false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.deathDuration && NetworkServer.active && this.deathExplosion)
            {
                //EffectManager.SpawnEffect(this.deathExplosion, new EffectData
                //{
                //    origin = base.transform.position,
                //    scale = 2f
                //}, true);

                EntityState.Destroy(base.gameObject);
            }
        }

        public override void OnExit()
        {
            base.DestroyModel();
            base.OnExit();
        }
    }
}
