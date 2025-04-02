using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;
using RoR2.Networking;
using UnityEngine.Events;
using UnityEngine.Networking;
using Unity;
using R2API.Networking.Interfaces;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class DeployTurret : BaseSkillState
    {
        public static float baseDuration = 0.1f;

        private float duration;

        private DeployTurret.PlacementInfo currentPlacementInfo;

        private struct PlacementInfo
        {
            public bool ok;
            public Vector3 position;
            public Quaternion rotation;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = DeployTurret.baseDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(2f);

            this.Fire();

            Debug.Log("!!!! SummonTurret OnEnter");
        }

        public override void OnExit()
        {
            base.OnExit();

            Debug.Log("!!!! SummonTurret OnExit");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private DeployTurret.PlacementInfo GetPlacementInfo()
        {
            Ray aimRay = base.GetAimRay();
            Vector3 direction = aimRay.direction;
            direction.y = 0f;
            direction.Normalize();
            aimRay.direction = direction;
            DeployTurret.PlacementInfo placementInfo = default(DeployTurret.PlacementInfo);
            placementInfo.ok = false;
            placementInfo.rotation = Util.QuaternionSafeLookRotation(-direction);
            Ray ray = new Ray(aimRay.GetPoint(2f) + Vector3.up * 1f, Vector3.down);
            float num = 4f;
            float num2 = num;
            RaycastHit raycastHit;
            if (Physics.SphereCast(ray, 0.5f, out raycastHit, num, LayerIndex.world.mask) && raycastHit.normal.y > 0.5f)
            {
                num2 = raycastHit.distance;
                placementInfo.ok = true;
            }
            Vector3 point = ray.GetPoint(num2 + 0.5f);
            placementInfo.position = point;
            if (placementInfo.ok)
            {
                float num3 = Mathf.Max(1.82f, 0f);
                if (Physics.CheckCapsule(placementInfo.position + Vector3.up * (num3 - 0.5f), placementInfo.position + Vector3.up * 0.5f, 0.45f, LayerIndex.world.mask | LayerIndex.CommonMasks.characterBodiesOrDefault))
                {
                    placementInfo.ok = false;
                }
            }
            return placementInfo;
        }

        private void Fire()
        {
            if (base.isAuthority)
            {
                if (base.characterBody)
                {
                    Debug.Log("Deploying turret...");
                    this.currentPlacementInfo = GetPlacementInfo();
                    SendConstructTurret(base.characterBody, this.currentPlacementInfo.position, this.currentPlacementInfo.rotation, MasterCatalog.FindMasterIndex(PonePack.Survivors.Hazel.hazelTurretMaster));
                }
            }
        }

        private class ConstructTurretMessage : MessageBase
        {
            public GameObject builder;

            public Vector3 position;

            public Quaternion rotation;

            public MasterCatalog.NetworkMasterIndex turretMasterIndex;

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(builder);
                writer.Write(position);
                writer.Write(rotation);
                GeneratedNetworkCode._WriteNetworkMasterIndex_MasterCatalog(writer, turretMasterIndex);
            }

            public override void Deserialize(NetworkReader reader)
            {
                builder = reader.ReadGameObject();
                position = reader.ReadVector3();
                rotation = reader.ReadQuaternion();
                turretMasterIndex = GeneratedNetworkCode._ReadNetworkMasterIndex_MasterCatalog(reader);
            }
        }

        [Client]
        public void SendConstructTurret(CharacterBody builder, Vector3 position, Quaternion rotation, MasterCatalog.MasterIndex masterIndex)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[Client] function 'System.Void RoR2.CharacterBody::SendConstructTurret(RoR2.CharacterBody,UnityEngine.Vector3,UnityEngine.Quaternion,RoR2.MasterCatalog/MasterIndex)' called on server");
                return;
            }

            ConstructTurretMessage msg = new ConstructTurretMessage
            {
                builder = builder.gameObject,
                position = position,
                rotation = rotation,
                turretMasterIndex = masterIndex
            };
            Debug.Log("Sent");
            ClientScene.readyConnection.Send(64, msg);
        }

        [NetworkMessageHandler(msgType = 64, server = true)]
        public static void HandleConstructTurret(NetworkMessage netMsg)
        {
            ConstructTurretMessage constructTurretMessage = netMsg.ReadMessage<ConstructTurretMessage>();
            if (!constructTurretMessage.builder)
            {
                return;
            }

            CharacterBody component = constructTurretMessage.builder.GetComponent<CharacterBody>();
            if ((bool)component)
            {
                CharacterMaster characterMaster = component.master;
                if ((bool)characterMaster)
                {
                    CharacterMaster characterMaster2 = new MasterSummon
                    {
                        masterPrefab = MasterCatalog.GetMasterPrefab(constructTurretMessage.turretMasterIndex),
                        position = constructTurretMessage.position,
                        rotation = constructTurretMessage.rotation,
                        summonerBodyObject = component.gameObject,
                        ignoreTeamMemberLimit = true,
                        inventoryToCopy = characterMaster.inventory
                    }.Perform();
                    Deployable deployable = characterMaster2.gameObject.AddComponent<Deployable>();
                    deployable.onUndeploy = new UnityEvent();
                    deployable.onUndeploy.AddListener(characterMaster2.TrueKill);
                    characterMaster.AddDeployable(deployable, PonePack.Survivors.Hazel.hazelTurretDeployableSlot);
                }
            }
        }
    }
}
