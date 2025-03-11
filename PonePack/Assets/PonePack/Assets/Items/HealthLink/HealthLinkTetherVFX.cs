using System;
using UnityEngine;
using RoR2;

namespace PonePack
{
    [RequireComponent(typeof(PP_BezierCurveLine))]
    public class HealthLinkTetherVFX : MonoBehaviour
    {
        private void Start()
        {
            this.curve = base.GetComponent<PP_BezierCurveLine>();
        }

        public void Update()
        {
            if (this.tetherTargetTransform && this.tetherEndTransform)
            {
                this.tetherEndTransform.position = this.tetherTargetTransform.position;
            }
            if (this.curve && this.tetherTargetTransform)
            {
                this.curve.p1 = this.tetherTargetTransform.position;
            }
        }

        public void Terminate()
        {
            Debug.Log("Terminate called!");
            //if (this.fadeOut)
            //{
            //    Debug.Log("FadeOut is enabled");
            //    this.fadeOut.enabled = true;
            //    return;
            //}
            UnityEngine.Object.Destroy(base.gameObject);
        }

        public AnimateShaderAlpha fadeOut;

        [Tooltip("The transform to position at the target.")]
        public Transform tetherEndTransform;

        [Tooltip("The transform to position the end to.")]
        public Transform tetherTargetTransform;

        private PP_BezierCurveLine curve;
    }
}
