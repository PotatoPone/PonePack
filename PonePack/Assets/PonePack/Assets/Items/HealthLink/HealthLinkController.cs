using HG;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using System.Collections.ObjectModel;
using R2API;

namespace PonePack
{
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    public class HealthLinkController : NetworkBehaviour
    {
        private int charactersInRange;
        private int maxCharacterCount; //Unneeded
        private float maxRange = 20f;
        private List<TeamComponent> teamMembersInRange;
        private List<Transform> tetheredTransforms;

        [SerializeField]
        public SphereCollider sphereCollider;

        [SerializeField]
        private GameObject indicatorSphere;

        [SerializeField]
        private NetworkedBodyAttachment networkedBodyAttachment;

        [SerializeField]
        private HealthLinkTetherVFXOrigin healthLinkTetherVFXOrigin;

        [SerializeField]
        private float syncInterval = 1f;

        [HideInInspector]
        public CharacterBody body;

        private float radiusSizeGrowth;
        private float timer;

        private void Awake()
        {
            this.networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();
        }

        private void Start()
        {
            Debug.Log("HealthLinkController starting...");

            this.body = this.networkedBodyAttachment.attachedBody;
            this.teamMembersInRange = new List<TeamComponent>();
            this.tetheredTransforms = new List<Transform>();
            bool active = NetworkServer.active;

            if (!active)
            {
                CharacterBody characterBody = this.body;
                characterBody.OnNetworkItemBehaviorUpdate = (Action<CharacterBody.NetworkItemBehaviorData>)Delegate.Combine(characterBody.OnNetworkItemBehaviorUpdate, new Action<CharacterBody.NetworkItemBehaviorData>(this.HandleNetworkItemUpdateClient));
            }
            else
            {
                this.body.onInventoryChanged += this.ServerUpdateValuesFromInventory;
                this.timer = this.syncInterval;
            }
            //this.TryUpdateCharactersInRange(20f);
            if (active)
            {
                //this.ReconcileBuffCount();
                this.ServerUpdateValuesFromInventory();
                return;
            }
            float indicatorDiameter;
            this.UpdateValues(this.body.inventory.GetItemCount(PonePack.Items.HealthLink), out indicatorDiameter);

            this.SetIndicatorDiameter(indicatorDiameter);

            //UpdateTeamMembersInRange();

            //this.gameObject.layer = LayerIndex.collideWithCharacterHullOnly.mask;
        }

        private void OnEnable()
        {
            On.RoR2.HealthComponent.Heal += HealAllies;
            On.RoR2.HealthComponent.TakeDamage += DamageAllies;
        }

        private void OnDisable()
        {
            On.RoR2.HealthComponent.Heal -= HealAllies;
            On.RoR2.HealthComponent.TakeDamage -= DamageAllies;

            if (this.body)
            {
                if (NetworkServer.active)
                {
                    this.body.onInventoryChanged -= this.ServerUpdateValuesFromInventory;
                    if (this.body.HasBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff))
                    {
                        this.body.SetBuffCount(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex, 0);
                        return;
                    }
                }
                else
                {
                    CharacterBody characterBody = this.body;
                    characterBody.OnNetworkItemBehaviorUpdate = (Action<CharacterBody.NetworkItemBehaviorData>)Delegate.Remove(characterBody.OnNetworkItemBehaviorUpdate, new Action<CharacterBody.NetworkItemBehaviorData>(this.HandleNetworkItemUpdateClient));
                }
            }
        }

        private void Update()
        {
            if (!NetworkServer.active) return;
            this.timer -= Time.deltaTime;
            if (this.timer > 0f) return;
            this.timer = this.syncInterval;

            UpdateTeamMembersInRange();
            //this.TryUpdateCharactersInRange(this.sphereCollider.radius);
            //this.ReconcileBuffCount();
        }

        private void UpdateTeamMembersInRange()
        {
            Debug.Log("UpdateTeamMembersInRange called!");
            teamMembersInRange.Clear();
            tetheredTransforms.Clear();

            ReadOnlyCollection<TeamComponent> teamComponents = TeamComponent.GetTeamMembers(TeamIndex.Player);

            foreach (TeamComponent teamComponent in teamComponents)
            {
                if (teamComponent.body == this.body) continue;
                if (Vector3.Distance(teamComponent.body.corePosition, this.body.corePosition) > maxRange) continue;

                teamMembersInRange.Add(teamComponent);
                tetheredTransforms.Add(teamComponent.body.coreTransform);
            }

            if (this.healthLinkTetherVFXOrigin)
            {
                Debug.Log("SetTetheredTransforms called");
                this.healthLinkTetherVFXOrigin.SetTetheredTransforms(tetheredTransforms);
            }
        }

        private bool CanAffectCharacter(HealthComponent healthComponent, ProcChainMask procChainMask)
        {
            if (!healthComponent.body) return false;
            if (!healthComponent.alive) return false;
            if (R2API.ProcTypeAPI.HasModdedProc(procChainMask, PonePack.ModdedProcTypes.HealthLink) == true) return false;

            return true;
        }

        private float HealAllies(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            Debug.Log("HealAllies was called");

            //-- code that will run before the original method

            float healAmount = orig(self, amount, procChainMask, nonRegen);

            //-- code that will run after the original method

            if (self.body && self.alive && self.body == this.body)
            {
                if (nonRegen == false) return 0; // Do not share healing from regen
                if (R2API.ProcTypeAPI.HasModdedProc(procChainMask, PonePack.ModdedProcTypes.HealthLink) == true) return 0; // Do not heal if this healing was caused by another shareHealth item

                UpdateTeamMembersInRange();

                foreach (TeamComponent teamComponent in teamMembersInRange)
                {
                    R2API.ProcTypeAPI.AddModdedProc(ref procChainMask, PonePack.ModdedProcTypes.HealthLink); // Prevent recursion
                    Debug.Log("Healing " + teamComponent.body.name);
                    teamComponent.body.healthComponent.Heal(healAmount, procChainMask, true);
                }
            }

            return healAmount;
        }

        private void DamageAllies(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (self.body && self.alive && self.body == this.body)
            {
                if (R2API.ProcTypeAPI.HasModdedProc(damageInfo.procChainMask, PonePack.ModdedProcTypes.HealthLink) == true) return; // Do not heal if this healing was caused by another shareHealth item

                UpdateTeamMembersInRange();

                foreach (TeamComponent teamComponent in teamMembersInRange)
                {
                    R2API.ProcTypeAPI.AddModdedProc(ref damageInfo.procChainMask, PonePack.ModdedProcTypes.HealthLink); // Prevent recursion
                    teamComponent.body.healthComponent.TakeDamage(damageInfo);
                }
            }
        }

        private void HandleNetworkItemUpdateClient(CharacterBody.NetworkItemBehaviorData itemBehaviorData)
        {
            if (itemBehaviorData.itemIndex != PonePack.Items.HealthLink.itemIndex) return;

            this.SetIndicatorDiameter(itemBehaviorData.floatValue);
        }

        private void ServerUpdateValuesFromInventory()
        {
            int itemCount = this.body.inventory.GetItemCount(PonePack.Items.HealthLink);
            float num;
            this.UpdateValues(itemCount, out num);
            this.SetIndicatorDiameter(num);
            this.body.TransmitItemBehavior(new CharacterBody.NetworkItemBehaviorData(PonePack.Items.HealthLink.itemIndex, num));
        }

        private void UpdateValues(int itemCount, out float diameter)
        {
            this.maxCharacterCount = 2 + itemCount * 2;
            this.radiusSizeGrowth = Util.ConvertAmplificationPercentageIntoReductionPercentage((float)(itemCount * 5));
            diameter = 35f + this.radiusSizeGrowth;
        }

        private void SetIndicatorDiameter(float diameter)
        {
            this.indicatorSphere.transform.localScale = new Vector3(maxRange * 2, maxRange * 2, maxRange * 2);
            this.sphereCollider.radius = maxRange;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Before active check");
            Debug.Log(other);
            if (!NetworkServer.active) return;
            Debug.Log("After active check");

            // Failing on check 2: other.gameObject.TryGetComponent<CharacterBody>(out otherBody)

            if (other != null)
            {
                Debug.Log("1: " + other);
                Debug.Log("2: " + other.gameObject.name);
                Debug.Log("3: " + other.gameObject.GetComponent<CharacterBody>() != null);
            }

            CharacterBody otherBody;
            if (other != null && other.gameObject.TryGetComponent<CharacterBody>(out otherBody) && this.CharacterBodyCountsTowardBuff(otherBody))
            {
                this.charactersInRange++;
                if (this.charactersInRange <= this.maxCharacterCount)
                {
                    this.body.AddBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff);
                }
                this.timer = this.syncInterval;

                UpdateTeamMembersInRange();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!NetworkServer.active) return;

            CharacterBody otherBody;
            if (other != null && other.gameObject.TryGetComponent<CharacterBody>(out otherBody) && this.CharacterBodyCountsTowardBuff(otherBody))
            {
                if (this.charactersInRange <= this.maxCharacterCount)
                {
                    this.body.RemoveBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff);
                }
                this.charactersInRange--;
                this.timer = this.syncInterval;

                UpdateTeamMembersInRange();
            }
        }

        //private bool TryUpdateCharactersInRange(float radius)
        //{
        //    TeamMask mask = default(TeamMask);
        //    mask.AddTeam(TeamIndex.Player);
        //    List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
        //    SphereSearch sphereSearch = new SphereSearch();
        //    sphereSearch.mask = LayerIndex.entityPrecise.mask;
        //    sphereSearch.origin = base.transform.position;
        //    sphereSearch.radius = radius;
        //    sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
        //    sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
        //    sphereSearch.RefreshCandidates();
        //    sphereSearch.OrderCandidatesByDistance();
        //    sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
        //    sphereSearch.GetHurtBoxes(list);
        //    sphereSearch.ClearCandidates();
        //    int num = 0;
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        HurtBox hurtBox = list[i];
        //        if (!(hurtBox.healthComponent == null) && !(hurtBox.healthComponent.body == null) && this.CharacterBodyCountsTowardBuff(hurtBox.healthComponent.body))
        //        {
        //            num++;
        //        }
        //    }
        //    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
        //    bool result = num != this.charactersInRange;
        //    this.charactersInRange = num;
        //    return result;
        //}

        //private void ReconcileBuffCount()
        //{
        //    if (!NetworkServer.active)
        //    {
        //        return;
        //    }
        //    BuffIndex buffIndex = PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex;
        //    int num = this.body.GetBuffCount(buffIndex);
        //    int num2 = Mathf.Min(this.maxCharacterCount, this.charactersInRange);
        //    int num3 = 0;
        //    while (num2 != num && num3 < 1000)
        //    {
        //        num3++;
        //        if (num > num2)
        //        {
        //            num--;
        //            this.body.RemoveBuff(buffIndex);
        //        }
        //        else if (num < num2)
        //        {
        //            num++;
        //            this.body.AddBuff(buffIndex);
        //        }
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CharacterBodyCountsTowardBuff(CharacterBody otherBody)
        {
            return otherBody != this.body && otherBody.healthComponent != null && otherBody.healthComponent.alive;
        }
    }
}
