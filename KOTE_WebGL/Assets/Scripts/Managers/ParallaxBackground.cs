using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Vector2 maxDistance = new Vector2(20, 10);
    public Vector2 center = new Vector2(0, -10);
    public float easing = 1;
    public Layer[] layers;
    public Layer[] rotateLayers;

    // Update is called once per frame
    void Update()
    {
        UpdatePositions(Camera.main.ScreenToViewportPoint(Input.mousePosition));
    }

    private void UpdatePositions(Vector3 delta)
    {
        delta = Vector3.ClampMagnitude(delta, 1.4f);
        foreach (var l in layers)
        {
            l.transform.localPosition =
                Vector3.Lerp(l.transform.localPosition,
                    new Vector3(
                        center.x + maxDistance.x * l.delta * delta.x,
                        center.y + maxDistance.y * l.delta * delta.y,
                        l.transform.localPosition.z),
                    Time.deltaTime * easing
                );
        }
        
        foreach (var l in rotateLayers)
        {
            l.transform.forward = transform.forward +
                                  new Vector3(
                                      l.delta * delta.x,
                                      l.delta * delta.y,
                                      0);
                ;
        }
    }

    [Serializable]
    public class Layer
    {
        public Transform transform;
        public float delta = 1;
    }
}
