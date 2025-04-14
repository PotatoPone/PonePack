using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using On;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace Druid
{
    public class StickToCharacter : MonoBehaviour
    {
        private SphereSearch sphereSearch;
        private List<HurtBox> foundHurtBoxes;
        private HurtBox chosenHurtBox;
        private CharacterBody victimCharacterBody;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            On.RoR2.CharacterMaster.OnBodyDeath += OnBodyDeath;
        }

        private void OnDisable()
        {
            On.RoR2.CharacterMaster.OnBodyDeath -= OnBodyDeath;
        }

        public void Stick(CharacterBody victimBody, Transform newParent)
        {
            this.victimCharacterBody = victimBody;
            Debug.Log("Victim: " + this.victimCharacterBody);
            if (!this.victimCharacterBody) return;

            transform.SetParent(newParent, true);
        }

        private void OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (this.victimCharacterBody && this.victimCharacterBody == body)
            {
                transform.SetParent(null, true);
                gameObject.GetComponent<HealthComponent>().Die();
            }

            orig(self, body);
        }
    }
}
