using UnityEngine;
using UnityEngine.Serialization;

public class SplineFromPointToPoint : MonoBehaviour
{
[SerializeField] private Transform initialPoint;
[SerializeField] private Transform targetPoint;
[SerializeField] private int numberOfPoints = 20;
[SerializeField] private LineRenderer lineRenderer;

[SerializeField] private Vector3 initialTangent = new Vector3(0, 1, 0);
[SerializeField] private Vector3 targetTangent = new Vector3(1, 0, 0);

[SerializeField] private float fDistance = 1;
[SerializeField] private float fSpeed = 1;
[SerializeField] private float fDelta = 0;
[SerializeField] private float animationSpeed = 1;

public AnimationCurve curve;

// Cache the line array to prevent memory allocation in the update loop
private Vector3[] lineArray;

void Start()
{
    // Cache the line array
    lineArray = new Vector3[numberOfPoints];

    // Get the line renderer component
    if (lineRenderer == null)
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    lineRenderer.positionCount = numberOfPoints;
    lineRenderer.SetPositions(CalculateLineArray(initialPoint.position, targetPoint.position));
}

private void Update()
{
    lineRenderer.SetPositions(CalculateLineArray(initialPoint.position, targetPoint.position));
}

private Vector3[] CalculateLineArray(Vector3 initialPos, Vector3 targetPos)
{
    // Renamed variables for clarity
    for (int i = 0; i < numberOfPoints; i++)
    {
        float t = i / (float)(numberOfPoints - 1);
        lineArray[i] = CalculateBezierPoint(t, initialPos, initialTangent, targetPos, targetTangent);
        lineArray[i] += Vector3.Lerp( Mathf.Clamp(initialPos.x - targetPos.x, -1,1) * Vector3.right, Vector3.up, t) * (fDistance * curve.Evaluate(t) * Mathf.Cos((-Time.time*animationSpeed + i) * fSpeed + fDelta));
    }
    return lineArray;
}

private Vector3 CalculateBezierPoint(float t, Vector3 initialPos, Vector3 initialTan, Vector3 targetPos, Vector3 targetTan)
{
    // Renamed variables for clarity and merged the calculation into a single line for performance
    return Mathf.Pow(1 - t, 3) * initialPos + 3 * t * Mathf.Pow(1 - t, 2) * initialTan + 3 * Mathf.Pow(t, 2) * (1 - t) * targetTan + Mathf.Pow(t, 3) * targetPos;
}
}