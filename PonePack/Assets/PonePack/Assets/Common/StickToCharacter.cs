using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using On;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace PonePack
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
            //foundHurtBoxes = new List<HurtBox>();

            //sphereSearch = new SphereSearch();
            //this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
            //this.sphereSearch.origin = this.transform.position;
            //this.sphereSearch.radius = this.transform.localScale.x; //Need to divde by two to be accurate
            //this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            //this.sphereSearch.RefreshCandidates();
            ////this.sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamIndex.Player));
            //this.sphereSearch.OrderCandidatesByDistance();
            //this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            //this.sphereSearch.GetHurtBoxes(foundHurtBoxes);
            //this.sphereSearch.ClearCandidates();

            //foreach (HurtBox hurtBox in foundHurtBoxes)
            //{
            //    //Don't attach if there's no healthComponent, or the healthComponent is dead
            //    if (!hurtBox.healthComponent) continue;
            //    if (hurtBox.healthComponent.alive == false) continue;

            //    this.chosenHurtBox = foundHurtBoxes[0];
            //    this.victimCharacterBody = this.chosenHurtBox.healthComponent.gameObject.GetComponent<CharacterBody>();

            //    transform.SetParent(this.chosenHurtBox.transform, true);
            //}
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
