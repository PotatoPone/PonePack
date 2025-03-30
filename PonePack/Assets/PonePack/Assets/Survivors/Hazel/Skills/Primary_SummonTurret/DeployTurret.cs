using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class DeployTurret : BaseSkillState
    {
        [SerializeField]
        public GameObject turretPrefab;

        [SerializeField]
        public GameObject turretMasterPrefab;

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
                    base.characterBody.SendConstructTurret(base.characterBody, this.currentPlacementInfo.position, this.currentPlacementInfo.rotation, MasterCatalog.FindMasterIndex(PonePack.MasterPrefabs.HazelTurretMaster));
                }
            }
        }
    }
}
