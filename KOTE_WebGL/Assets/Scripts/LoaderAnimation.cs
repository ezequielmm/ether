using UnityEngine;

public class LoaderAnimation : MonoBehaviour
{
    [SerializeField] private int segments = 50;
    [SerializeField] private float radius = 1.0f;
    [SerializeField] private float rotationSpeed = 100.0f;
    [SerializeField] private float lineWidthVariationSpeed = 1.0f;

    private LineRenderer lineRenderer;
    [SerializeField] private float delta;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;

        CreateCircle();
    }

    private void Update()
    {
        RotateLoader();
        UpdateLineWidth();
    }

    private void CreateCircle()
    {
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));

            angle += (2 * Mathf.PI) / segments;
        }

        // Set the last position to match the first position
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, lineRenderer.GetPosition(2));
    }

    private void RotateLoader()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void UpdateLineWidth()
    {
        float angle = (Time.time + delta) * lineWidthVariationSpeed;
        float minLineWidth = -.5f;
        float maxLineWidth = .5f;

        for (int i = 0; i <= segments; i++)
        {
            var sin = ((Mathf.Sin(angle + (2 * Mathf.PI) / segments * i)) + 1) / 2;
            float width = Mathf.Lerp(minLineWidth, maxLineWidth, sin);
            lineRenderer.widthCurve = AnimationCurve.Linear(0, width, 1, width);

            angle += (2 * Mathf.PI) / segments;
        }
    }
}