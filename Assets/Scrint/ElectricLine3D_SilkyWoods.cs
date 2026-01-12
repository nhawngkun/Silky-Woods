using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ElectricLine3D_SilkyWoods : MonoBehaviour
{
    public Transform startObj; 
    public Transform endObj;   

    private Vector3 fixedEndPos;
    private bool isFixed = false;

    [Header("Cấu hình độ giật")]
    public int pointsCount = 10; 
    public float jitterAmount = 0.5f; 

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector3 startPos = startObj != null ? startObj.position : transform.position;
        Vector3 endPos = isFixed ? fixedEndPos : (endObj != null ? endObj.position : transform.position);

        DrawLightning(startPos, endPos);
    }

    void DrawLightning(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPosition(0, start);

        for (int i = 1; i < pointsCount - 1; i++)
        {
            float lerpVal = (float)i / (pointsCount - 1);
            Vector3 pointOnLine = Vector3.Lerp(start, end, lerpVal);
            Vector3 randomJitter = Random.insideUnitSphere * jitterAmount;
            lineRenderer.SetPosition(i, pointOnLine + randomJitter);
        }

        lineRenderer.SetPosition(pointsCount - 1, end);
    }

    public void LockLine(Vector3 finalPosition)
    {
        isFixed = true;
        fixedEndPos = finalPosition;
        endObj = null;
    }

    public void SetLineMaterial(Material newMat)
    {
        if (lineRenderer != null && newMat != null)
        {
            lineRenderer.material = newMat;
        }
    }
}