using HG;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace PonePack
{
    public class ShareHealthChangesWithNearbyAllies : MonoBehaviour
    {
        private int charactersInRange;
        private int maxCharacterCount;

        [SerializeField]
        public SphereCollider sphereCollider;

        [SerializeField]
        private GameObject indicatorSphere;

        [SerializeField]
        private NetworkedBodyAttachment networkedBodyAttachment;

        [SerializeField]
        private float syncInterval = 3f;

        [HideInInspector]
        public CharacterBody body;

        private float radiusSizeGrowth;
        private float timer;

        private void Start()
        {
            this.networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();

            Debug.Log("networkedBodyAttachment: " + this.networkedBodyAttachment);

            this.body = this.networkedBodyAttachment.attachedBody; //Obj ref not set to instance
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
            this.TryUpdateCharactersInRange(20f);
            if (active)
            {
                this.ReconcileBuffCount();
                this.ServerUpdateValuesFromInventory();
                return;
            }
            float indicatorDiameter;
            this.UpdateValues(this.body.inventory.GetItemCount(PonePack.Items.ShareHealthChangesWithNearbyAllies), out indicatorDiameter);

            this.SetIndicatorDiameter(indicatorDiameter);
        }

        private void Update()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            this.timer -= Time.deltaTime;
            if (this.timer > 0f)
            {
                return;
            }
            this.timer = this.syncInterval;
            this.TryUpdateCharactersInRange(this.sphereCollider.radius);
            this.ReconcileBuffCount();
        }

        private void HandleNetworkItemUpdateClient(CharacterBody.NetworkItemBehaviorData itemBehaviorData)
        {
            if (itemBehaviorData.itemIndex != PonePack.Items.ShareHealthChangesWithNearbyAllies.itemIndex)
            {
                return;
            }
            this.SetIndicatorDiameter(itemBehaviorData.floatValue);
        }

        private void ServerUpdateValuesFromInventory()
        {
            int itemCount = this.body.inventory.GetItemCount(PonePack.Items.ShareHealthChangesWithNearbyAllies);
            float num;
            this.UpdateValues(itemCount, out num);
            this.SetIndicatorDiameter(num);
            this.body.TransmitItemBehavior(new CharacterBody.NetworkItemBehaviorData(PonePack.Items.ShareHealthChangesWithNearbyAllies.itemIndex, num));
        }

        private void UpdateValues(int itemCount, out float diameter)
        {
            this.maxCharacterCount = 2 + itemCount * 2;
            this.radiusSizeGrowth = Util.ConvertAmplificationPercentageIntoReductionPercentage((float)(itemCount * 5));
            diameter = 35f + this.radiusSizeGrowth;
        }

        private void SetIndicatorDiameter(float diameter)
        {
            this.indicatorSphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            this.sphereCollider.radius = diameter / 2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            CharacterBody enemyBody;
            if (other != null && other.gameObject.TryGetComponent<CharacterBody>(out enemyBody) && this.CharacterBodyCountsTowardBuff(enemyBody))
            {
                this.charactersInRange++;
                if (this.charactersInRange <= this.maxCharacterCount)
                {
                    Debug.Log("A valid character has entered range!");
                    this.body.AddBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff);
                }
                this.timer = this.syncInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            CharacterBody enemyBody;
            if (other != null && other.gameObject.TryGetComponent<CharacterBody>(out enemyBody) && this.CharacterBodyCountsTowardBuff(enemyBody))
            {
                if (this.charactersInRange <= this.maxCharacterCount)
                {
                    this.body.RemoveBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff);
                }
                this.charactersInRange--;
                this.timer = this.syncInterval;
            }
        }

        private bool TryUpdateCharactersInRange(float radius)
        {
            List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = base.transform.position;
            sphereSearch.radius = radius;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(list);
            sphereSearch.ClearCandidates();
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                HurtBox hurtBox = list[i];
                if (!(hurtBox.healthComponent == null) && !(hurtBox.healthComponent.body == null) && this.CharacterBodyCountsTowardBuff(hurtBox.healthComponent.body))
                {
                    num++;
                }
            }
            CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
            bool result = num != this.charactersInRange;
            this.charactersInRange = num;
            return result;
        }

        private void ReconcileBuffCount()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            BuffIndex buffIndex = PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex;
            int num = this.body.GetBuffCount(buffIndex);
            int num2 = Mathf.Min(this.maxCharacterCount, this.charactersInRange);
            int num3 = 0;
            while (num2 != num && num3 < 1000)
            {
                num3++;
                if (num > num2)
                {
                    num--;
                    this.body.RemoveBuff(buffIndex);
                }
                else if (num < num2)
                {
                    num++;
                    this.body.AddBuff(buffIndex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CharacterBodyCountsTowardBuff(CharacterBody enemyBody)
        {
            return enemyBody != this.body && enemyBody.healthComponent != null && enemyBody.healthComponent.alive;
        }

        private void OnDisable()
        {
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
    }
}
