using System;
using UnityEngine;

// Token: 0x0200004C RID: 76
[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class PP_BezierCurveLine : MonoBehaviour
{
    public LineRenderer lineRenderer { get; private set; }

    private void Awake()
    {
        this.lineRenderer = base.GetComponent<LineRenderer>();
        this.windPhaseShift = UnityEngine.Random.insideUnitSphere * 360f;
        Array.Resize<Vector3>(ref this.vertexList, this.lineRenderer.positionCount + 1);
        this.UpdateBezier(0f);
    }

    public void OnEnable()
    {
        Array.Resize<Vector3>(ref this.vertexList, this.lineRenderer.positionCount + 1);
    }

    private void LateUpdate()
    {
        this.UpdateBezier(Time.deltaTime);
    }

    public void UpdateBezier(float deltaTime)
    {
        this.windTime += deltaTime;
        this.p0 = base.transform.position;
        if (this.endTransform)
        {
            this.p1 = this.endTransform.position;
        }
        if (this.animateBezierWind)
        {
            this.finalv0 = this.v0 + new Vector3(Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.x) * this.windFrequency.x) * this.windMagnitude.x, Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.y) * this.windFrequency.y) * this.windMagnitude.y, Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.z) * this.windFrequency.z) * this.windMagnitude.z);
            this.finalv1 = this.v1 + new Vector3(Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.x + this.p1.x) * this.windFrequency.x) * this.windMagnitude.x, Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.y + this.p1.z) * this.windFrequency.y) * this.windMagnitude.y, Mathf.Sin(0.017453292f * (this.windTime * 360f + this.windPhaseShift.z + this.p1.y) * this.windFrequency.z) * this.windMagnitude.z);
        }
        else
        {
            this.finalv0 = this.v0;
            this.finalv1 = this.v1;
        }
        for (int i = 0; i < this.vertexList.Length; i++)
        {
            float t = (float)i / (float)(this.vertexList.Length - 2);
            this.vertexList[i] = this.EvaluateBezier(t);
        }
        this.lineRenderer.SetPositions(this.vertexList);
    }

    private Vector3 EvaluateBezier(float t)
    {
        Vector3 a = Vector3.Lerp(this.p0, this.p0 + this.finalv0, t);
        Vector3 b = Vector3.Lerp(this.p1, this.p1 + this.finalv1, 1f - t);
        return Vector3.Lerp(a, b, t);
    }

    private Vector3[] vertexList = Array.Empty<Vector3>();

    private Vector3 p0 = Vector3.zero;

    public Vector3 v0 = Vector3.zero;

    public Vector3 p1 = Vector3.zero;

    public Vector3 v1 = Vector3.zero;

    public Transform endTransform;

    public bool animateBezierWind;

    public Vector3 windMagnitude;

    public Vector3 windFrequency;

    private Vector3 windPhaseShift;

    private Vector3 lastWind;

    private Vector3 finalv0;

    private Vector3 finalv1;

    private float windTime;
}
