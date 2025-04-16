using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using On;
using RoR2;
using System;
using UnityEngine.Networking;

namespace Druid
{
    public class DruidTurretController : MonoBehaviour
    {
        public float healRadius = 20f;

        private float storedHealing;
        private float storedDamage;
        private HealthComponent healthComponent;

        private void OnEnable()
        {
            healthComponent = GetComponent<HealthComponent>();

            //On.RoR2.HealthComponent.Heal += OnHeal;
            //On.RoR2.HealthComponent.TakeDamage += OnTakeDamage;
            GlobalEventManager.onServerDamageDealt += OnDamageDealt;
        }


        private void OnDisable()
        {
            //On.RoR2.HealthComponent.Heal -= OnHeal;
            //On.RoR2.HealthComponent.TakeDamage -= OnTakeDamage;
            GlobalEventManager.onServerDamageDealt -= OnDamageDealt;
        }

        public float GetStoredHealing()
        {
            return storedHealing;
        }

        public float GetStoredDamage()
        {
            return storedDamage;
        }

        private void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (self == healthComponent)
            {
                storedDamage += damageInfo.damage;
            }
        }

        private void OnDamageDealt(DamageReport report)
        {
            if (report.victim != healthComponent) return;

            storedDamage += report.damageDealt;
        }

        private float OnHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            //Override ability to heal
            if (self == healthComponent)
            {
                if (!self.alive || amount <= 0f || self.body.HasBuff(RoR2Content.Buffs.HealingDisabled))
                {
                    return 0f;
                }

                storedHealing += amount;
                Debug.Log(amount + " healing was stored. Stored healing: " + storedHealing);
                return 0f;
            }
            else
            {
                return orig(self, amount, procChainMask, nonRegen);
            }
        }

        //private void OnDeath()
        //{
        //    Debug.LogWarning("Healing nearby allies for " + storedHealing);
        //    List<HurtBox> foundHurtBoxes = new List<HurtBox>();

        //    SphereSearch sphereSearch = new SphereSearch();
        //    sphereSearch.mask = LayerIndex.entityPrecise.mask;
        //    sphereSearch.origin = this.transform.position;
        //    sphereSearch.radius = healRadius;
        //    sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
        //    sphereSearch.RefreshCandidates();
        //    TeamMask teamMask = TeamMask.none;
        //    teamMask.AddTeam(TeamIndex.Player);
        //    sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
        //    sphereSearch.OrderCandidatesByDistance();
        //    sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
        //    sphereSearch.GetHurtBoxes(foundHurtBoxes);
        //    sphereSearch.ClearCandidates();

        //    foreach (HurtBox hurtBox in foundHurtBoxes)
        //    {
        //        if (!hurtBox.healthComponent) continue;
        //        if (hurtBox.healthComponent == healthComponent) continue; //Don't heal yourself
        //        if (hurtBox.healthComponent.alive == false) continue;

        //        //Add HealingFromDyingTurret ProcChainMask
        //        hurtBox.healthComponent.Heal(storedHealing, default(ProcChainMask), true);
        //    }
        //}
    }
}
