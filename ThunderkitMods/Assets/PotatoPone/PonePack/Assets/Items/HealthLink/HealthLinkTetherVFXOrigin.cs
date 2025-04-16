using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace PonePack
{
    public class HealthLinkTetherVFXOrigin : MonoBehaviour, ITargetListReceiver<HealthComponent>
    {
        //private protected new Transform transform { protected get; private set; }

        protected new Transform transform;
        public GameObject tetherPrefab;
        private List<Transform> tetheredTransforms;
        private List<HealthLinkTetherVFX> tetherVfxs;
        public HealthLinkTetherVFXOrigin.TetherAddDelegate onTetherAdded;
        public HealthLinkTetherVFXOrigin.TetherRemoveDelegate onTetherRemoved;
        public delegate void TetherAddDelegate(HealthLinkTetherVFX vfx, Transform transform);
        public delegate void TetherRemoveDelegate(HealthLinkTetherVFX vfx);

        protected void Awake()
        {
            this.transform = base.transform;
            this.tetheredTransforms = CollectionPool<Transform, List<Transform>>.RentCollection();
            this.tetherVfxs = CollectionPool<HealthLinkTetherVFX, List<HealthLinkTetherVFX>>.RentCollection();
        }

        protected void OnDestroy()
        {
            for (int i = this.tetherVfxs.Count - 1; i >= 0; i--)
            {
                this.RemoveTetherAt(i);
            }
            this.tetherVfxs = CollectionPool<HealthLinkTetherVFX, List<HealthLinkTetherVFX>>.ReturnCollection(this.tetherVfxs);
            this.tetheredTransforms = CollectionPool<Transform, List<Transform>>.ReturnCollection(this.tetheredTransforms);
        }

        protected void AddTether(Transform target)
        {
            if (!target)
            {
                return;
            }
            HealthLinkTetherVFX tetherVfx = null;
            if (this.tetherPrefab)
            {
                Debug.Log("Attempting to instantiate a teather...");
                tetherVfx = UnityEngine.Object.Instantiate<GameObject>(this.tetherPrefab, this.transform).GetComponent<HealthLinkTetherVFX>();
                tetherVfx.tetherTargetTransform = target;

                //NetworkServer.Spawn(tetherVfx.gameObject);
            }
            this.tetheredTransforms.Add(target);
            this.tetherVfxs.Add(tetherVfx);
            HealthLinkTetherVFXOrigin.TetherAddDelegate tetherAddDelegate = this.onTetherAdded;
            if (tetherAddDelegate == null)
            {
                return;
            }
            tetherAddDelegate(tetherVfx, target);
        }

        protected void RemoveTetherAt(int i)
        {
            HealthLinkTetherVFX tetherVfx = this.tetherVfxs[i];
            if (tetherVfx)
            {
                HealthLinkTetherVFXOrigin.TetherRemoveDelegate tetherRemoveDelegate = this.onTetherRemoved;
                if (tetherRemoveDelegate != null)
                {
                    tetherRemoveDelegate(tetherVfx);
                }
                tetherVfx.transform.SetParent(null);
                tetherVfx.Terminate();
            }
            this.tetheredTransforms.RemoveAt(i);
            this.tetherVfxs.RemoveAt(i);
        }

        public void SetTetheredTransforms(List<Transform> newTetheredTransforms)
        {
            List<Transform> list = CollectionPool<Transform, List<Transform>>.RentCollection();
            List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
            ListUtils.FindExclusiveEntriesByReference<Transform>(this.tetheredTransforms, newTetheredTransforms, list2, list);
            int i = 0;
            int count = list2.Count;
            while (i < count)
            {
                List<Transform> list3 = this.tetheredTransforms;
                Transform transform = list2[i];
                this.RemoveTetherAt(ListUtils.FirstOccurrenceByReference<List<Transform>, Transform>(list3, transform));
                i++;
            }
            int j = 0;
            int count2 = list.Count;
            while (j < count2)
            {
                this.AddTether(list[j]);
                j++;
            }
            CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
            CollectionPool<Transform, List<Transform>>.ReturnCollection(list);
        }

        public void UpdateTargets(ReadOnlyCollection<HealthComponent> listOfHealthComponents, ReadOnlyCollection<HealthComponent> discoveredHealthComponents, ReadOnlyCollection<HealthComponent> lostHealthComponents)
        {
            for (int i = this.tetheredTransforms.Count - 1; i >= 0; i--)
            {
                if (this.tetheredTransforms[i] == null)
                {
                    this.RemoveTetherAt(i);
                }
            }
            int j = 0;
            int count = lostHealthComponents.Count;
            while (j < count)
            {
                Transform transform = Util.HealthComponentToTransform(lostHealthComponents[j]);
                if (!(transform == null))
                {
                    this.RemoveTetherAt(ListUtils.FirstOccurrenceByReference<List<Transform>, Transform>(this.tetheredTransforms, transform));
                }
                j++;
            }
            int k = 0;
            int count2 = discoveredHealthComponents.Count;
            while (k < count2)
            {
                Transform transform = Util.HealthComponentToTransform(discoveredHealthComponents[k]);
                this.AddTether(transform);
                k++;
            }
        }

        public void SetTransform(Transform t)
        {
            if (t != null)
            {
                this.transform = t;
            }
        }
    }
}
