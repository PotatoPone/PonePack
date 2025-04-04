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
using RoR2.Projectile;
using static UnityEngine.UI.GridLayoutGroup;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class DeployTurret : BaseState
    {
        public static float baseDuration = 0.1f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = DeployTurret.baseDuration / this.attackSpeedStat;
            this.Deploy();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextState(new Idle());
                return;
            }
        }

        private void Deploy()
        {
            if (base.isAuthority)
            {
                ProjectileController projectileController = base.gameObject.GetComponent<ProjectileController>();
                if (!projectileController) return;
                CharacterBody owner = projectileController.owner.GetComponent<CharacterBody>();
                if (!owner) return;

                SendConstructTurret(owner, base.gameObject.transform.position, base.gameObject.transform.rotation, MasterCatalog.FindMasterIndex(PonePack.Survivors.Hazel.hazelTurretMaster));

                //test
                Destroy(base.gameObject);
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

        //!!!!!!!!!! remove static keyword
        [NetworkMessageHandler(msgType = 64, server = true)]
        public static void HandleConstructTurret(NetworkMessage netMsg)
        {
            ConstructTurretMessage constructTurretMessage = netMsg.ReadMessage<ConstructTurretMessage>();
            if (!constructTurretMessage.builder)
            {
                Debug.LogWarning("Couldn't construct turret: No builder");
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
