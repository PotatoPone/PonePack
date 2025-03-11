using System;
using UnityEngine;

// Token: 0x0200004C RID: 76
[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class PP_BezierCurveLine : MonoBehaviour
{
    // Token: 0x17000024 RID: 36
    // (get) Token: 0x06000182 RID: 386 RVA: 0x0000808F File Offset: 0x0000628F
    // (set) Token: 0x06000183 RID: 387 RVA: 0x00008097 File Offset: 0x00006297
    public LineRenderer lineRenderer { get; private set; }

    // Token: 0x06000184 RID: 388 RVA: 0x000080A0 File Offset: 0x000062A0
    private void Awake()
    {
        this.lineRenderer = base.GetComponent<LineRenderer>();
        this.windPhaseShift = UnityEngine.Random.insideUnitSphere * 360f;
        Array.Resize<Vector3>(ref this.vertexList, this.lineRenderer.positionCount + 1);
        this.UpdateBezier(0f);
    }

    // Token: 0x06000185 RID: 389 RVA: 0x000080F1 File Offset: 0x000062F1
    public void OnEnable()
    {
        Array.Resize<Vector3>(ref this.vertexList, this.lineRenderer.positionCount + 1);
    }

    // Token: 0x06000186 RID: 390 RVA: 0x0000810B File Offset: 0x0000630B
    private void LateUpdate()
    {
        this.UpdateBezier(Time.deltaTime);
    }

    // Token: 0x06000187 RID: 391 RVA: 0x00008118 File Offset: 0x00006318
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

    // Token: 0x06000188 RID: 392 RVA: 0x00008380 File Offset: 0x00006580
    private Vector3 EvaluateBezier(float t)
    {
        Vector3 a = Vector3.Lerp(this.p0, this.p0 + this.finalv0, t);
        Vector3 b = Vector3.Lerp(this.p1, this.p1 + this.finalv1, 1f - t);
        return Vector3.Lerp(a, b, t);
    }

    // Token: 0x04000185 RID: 389
    private Vector3[] vertexList = Array.Empty<Vector3>();

    // Token: 0x04000186 RID: 390
    private Vector3 p0 = Vector3.zero;

    // Token: 0x04000187 RID: 391
    public Vector3 v0 = Vector3.zero;

    // Token: 0x04000188 RID: 392
    public Vector3 p1 = Vector3.zero;

    // Token: 0x04000189 RID: 393
    public Vector3 v1 = Vector3.zero;

    // Token: 0x0400018A RID: 394
    public Transform endTransform;

    // Token: 0x0400018B RID: 395
    public bool animateBezierWind;

    // Token: 0x0400018C RID: 396
    public Vector3 windMagnitude;

    // Token: 0x0400018D RID: 397
    public Vector3 windFrequency;

    // Token: 0x0400018E RID: 398
    private Vector3 windPhaseShift;

    // Token: 0x0400018F RID: 399
    private Vector3 lastWind;

    // Token: 0x04000190 RID: 400
    private Vector3 finalv0;

    // Token: 0x04000191 RID: 401
    private Vector3 finalv1;

    // Token: 0x04000192 RID: 402
    private float windTime;
}
