using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;

namespace Druid
{
    public class StickToCharacter : NetworkBehaviour
    {
        public Transform stuckTransform { get; private set; }
        public CharacterBody stuckBody { get; private set; }

        private void Awake()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
        }

        private void OnEnable()
        {
            if (this.wasEverEnabled)
            {
                Collider component = base.GetComponent<Collider>();
                component.enabled = false;
                component.enabled = true;
            }
            this.wasEverEnabled = true;
        }

        private void OnDisable()
        {
            if (NetworkServer.active)
            {
                this.Detach();
            }
        }

        public GameObject victim
        {
            get
            {
                return this._victim;
            }
            private set
            {
                this._victim = value;
                this.NetworksyncVictim = value;
            }
        }

        public bool stuck
        {
            get
            {
                return this.hitHurtboxIndex != -1;
            }
        }

        [Server]
        public void Detach()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.Projectile.ProjectileStickOnImpact::Detach()' called on client");
                return;
            }
            this.victim = null;
            this.stuckTransform = null;
            this.NetworkhitHurtboxIndex = -1;
            this.UpdateSticking();
        }

        public void Stick(ProjectileImpactInfo impactInfo)
        {
            if (!base.enabled)
            {
                return;
            }
            this.TrySticking(impactInfo.collider, impactInfo.estimatedImpactNormal);
        }

        private bool TrySticking(Collider hitCollider, Vector3 impactNormal)
        {
            if (this.victim)
            {
                return false;
            }
            GameObject gameObject = null;
            sbyte networkhitHurtboxIndex = -1;
            HurtBox component = hitCollider.GetComponent<HurtBox>();
            if (component)
            {
                HealthComponent healthComponent = component.healthComponent;
                if (healthComponent)
                {
                    gameObject = healthComponent.gameObject;
                }
                networkhitHurtboxIndex = (sbyte)component.indexInGroup;
            }
            if (!gameObject && !this.ignoreWorld)
            {
                gameObject = hitCollider.gameObject;
                networkhitHurtboxIndex = -2;
            }
            //if (gameObject == this.projectileController.owner || (this.ignoreCharacters && component))
            //{
            //    gameObject = null;
            //    networkhitHurtboxIndex = -1;
            //}
            if (gameObject)
            {
                this.NetworkrunStickEvent = true;
                ParticleSystem[] array = this.stickParticleSystem;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Play();
                }
                if (this.stickSoundString.Length > 0)
                {
                    Util.PlaySound(this.stickSoundString, base.gameObject);
                }
                if (this.alignNormals && impactNormal != Vector3.zero)
                {
                    base.transform.rotation = Util.QuaternionSafeLookRotation(impactNormal, base.transform.up);
                }
                Transform transform = hitCollider.transform;
                this.NetworklocalPosition = transform.InverseTransformPoint(base.transform.position);
                this.NetworklocalRotation = Quaternion.Inverse(transform.rotation) * base.transform.rotation;
                this.victim = gameObject;
                this.NetworkhitHurtboxIndex = networkhitHurtboxIndex;
                return true;
            }
            return false;
        }

        private void FixedUpdate()
        {
            this.MyFixedUpdate(Time.deltaTime);
        }

        private void UpdateSticking()
        {
            bool flag = this.stuckTransform;
            if (flag)
            {
                base.transform.SetPositionAndRotation(this.stuckTransform.TransformPoint(this.localPosition), this.alignNormals ? (this.stuckTransform.rotation * this.localRotation) : base.transform.rotation);
            }
            else
            {
                GameObject gameObject = NetworkServer.active ? this.victim : this.syncVictim;
                if (gameObject)
                {
                    this.stuckTransform = gameObject.transform;
                    flag = true;
                    if (this.hitHurtboxIndex >= 0)
                    {
                        this.stuckBody = this.stuckTransform.GetComponent<CharacterBody>();
                        if (this.stuckBody && (int)this.hitHurtboxIndex < this.stuckBody.hurtBoxGroup.hurtBoxes.Length && this.stuckBody.hurtBoxGroup.hurtBoxes[(int)this.hitHurtboxIndex] != null)
                        {
                            this.stuckTransform = this.stuckBody.hurtBoxGroup.hurtBoxes[(int)this.hitHurtboxIndex].transform;
                        }
                        ModelLocator component = this.syncVictim.GetComponent<ModelLocator>();
                        if (component)
                        {
                            Transform modelTransform = component.modelTransform;
                            if (modelTransform)
                            {
                                HurtBoxGroup component2 = modelTransform.GetComponent<HurtBoxGroup>();
                                if (component2)
                                {
                                    HurtBox hurtBox = component2.hurtBoxes[(int)this.hitHurtboxIndex];
                                    if (hurtBox)
                                    {
                                        this.stuckTransform = hurtBox.transform;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (this.hitHurtboxIndex == -2 && !NetworkServer.active)
                {
                    flag = true;
                }
            }
            if (NetworkServer.active)
            {
                if (!flag)
                {
                    this.NetworkhitHurtboxIndex = -1;
                }
            }
        }

        public void MyFixedUpdate(float deltaTime)
        {
            this.UpdateSticking();
            if (!this.alreadyRanStickEvent && this.runStickEvent)
            {
                this.stickEvent.Invoke();
                this.alreadyRanStickEvent = true;
            }
        }

        private void UNetVersion()
        {
        }

        public GameObject NetworksyncVictim
        {
            get
            {
                return this.syncVictim;
            }
            [param: In]
            set
            {
                base.SetSyncVarGameObject(value, ref this.syncVictim, 1U, ref this.___syncVictimNetId);
            }
        }

        public sbyte NetworkhitHurtboxIndex
        {
            get
            {
                return this.hitHurtboxIndex;
            }
            [param: In]
            set
            {
                base.SetSyncVar<sbyte>(value, ref this.hitHurtboxIndex, 2U);
            }
        }

        public Vector3 NetworklocalPosition
        {
            get
            {
                return this.localPosition;
            }
            [param: In]
            set
            {
                base.SetSyncVar<Vector3>(value, ref this.localPosition, 4U);
            }
        }

        public Quaternion NetworklocalRotation
        {
            get
            {
                return this.localRotation;
            }
            [param: In]
            set
            {
                base.SetSyncVar<Quaternion>(value, ref this.localRotation, 8U);
            }
        }

        public bool NetworkrunStickEvent
        {
            get
            {
                return this.runStickEvent;
            }
            [param: In]
            set
            {
                base.SetSyncVar<bool>(value, ref this.runStickEvent, 16U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(this.syncVictim);
                writer.WritePackedUInt32((uint)this.hitHurtboxIndex);
                writer.Write(this.localPosition);
                writer.Write(this.localRotation);
                writer.Write(this.runStickEvent);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.syncVictim);
            }
            if ((base.syncVarDirtyBits & 2U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.WritePackedUInt32((uint)this.hitHurtboxIndex);
            }
            if ((base.syncVarDirtyBits & 4U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.localPosition);
            }
            if ((base.syncVarDirtyBits & 8U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.localRotation);
            }
            if ((base.syncVarDirtyBits & 16U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.runStickEvent);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.___syncVictimNetId = reader.ReadNetworkId();
                this.hitHurtboxIndex = (sbyte)reader.ReadPackedUInt32();
                this.localPosition = reader.ReadVector3();
                this.localRotation = reader.ReadQuaternion();
                this.runStickEvent = reader.ReadBoolean();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.syncVictim = reader.ReadGameObject();
            }
            if ((num & 2) != 0)
            {
                this.hitHurtboxIndex = (sbyte)reader.ReadPackedUInt32();
            }
            if ((num & 4) != 0)
            {
                this.localPosition = reader.ReadVector3();
            }
            if ((num & 8) != 0)
            {
                this.localRotation = reader.ReadQuaternion();
            }
            if ((num & 16) != 0)
            {
                this.runStickEvent = reader.ReadBoolean();
            }
        }

        public override void PreStartClient()
        {
            if (!this.___syncVictimNetId.IsEmpty())
            {
                this.NetworksyncVictim = ClientScene.FindLocalObject(this.___syncVictimNetId);
            }
        }

        public string stickSoundString;
        public ParticleSystem[] stickParticleSystem;
        public bool ignoreCharacters;
        public bool ignoreWorld;
        public bool alignNormals = true;
        public UnityEvent stickEvent;
        private CharacterBody characterBody;
        private bool wasEverEnabled;
        private GameObject _victim;

        [SyncVar]
        private GameObject syncVictim;
        [SyncVar]
        private sbyte hitHurtboxIndex = -1;
        [SyncVar]
        private Vector3 localPosition;
        [SyncVar]
        private Quaternion localRotation;
        [SyncVar]
        private bool runStickEvent;

        private bool alreadyRanStickEvent;
        private NetworkInstanceId ___syncVictimNetId;
    }
}
