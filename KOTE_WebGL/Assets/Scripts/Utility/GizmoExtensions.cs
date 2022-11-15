using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GizmoExtensions
{
    public static void DrawBox(Bounds bounds, Vector3 offset = default(Vector3))
    {
        offset += new Vector3(bounds.center.x, bounds.center.y, 0);
        DrawBox(
            new Vector3(bounds.extents.x + offset.x, bounds.extents.y + offset.y, offset.z),
            new Vector3(-bounds.extents.x + offset.x, bounds.extents.y + offset.y, offset.z),
            new Vector3(bounds.extents.x + offset.x, -bounds.extents.y + offset.y, offset.z),
            new Vector3(-bounds.extents.x + offset.x, -bounds.extents.y + offset.y, offset.z));
    }

    public static void DrawBox(float width, float height, Vector3 offset = default(Vector3))
    {
        DrawBox(
            new Vector3((width / 2) + offset.x, (height / 2) + offset.y, offset.z),
            new Vector3(-(width / 2) + offset.x, (height / 2) + offset.y, offset.z),
            new Vector3((width / 2) + offset.x, -(height / 2) + offset.y, offset.z),
            new Vector3(-(width / 2) + offset.x, -(height / 2) + offset.y, offset.z));
    }

    public static void DrawBox(Vector3 TopLeft, Vector3 TopRight, Vector3 BottomLeft, Vector3 BottomRight)
    {
        Gizmos.DrawLine(TopLeft, TopRight);
        Gizmos.DrawLine(TopRight, BottomRight);
        Gizmos.DrawLine(BottomRight, BottomLeft);
        Gizmos.DrawLine(BottomLeft, TopLeft);
    }
}
