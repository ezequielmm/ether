using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    public int id;
    public string type;
    public List<int> exitNodesIds = new List<int>();
    public Material lineMat;

    LineRenderer lineRenderer;
    private void Start()
    {
        
        //Debug.Log(lineRenderer);
    }

    public void OnNodeClick()
    {
        Debug.Log("Node clicked "+id);
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(id);
    }

    public void UpdateLine(GameObject targetOb)
    {
        Debug.Log("Updating line!");

        var child = new GameObject();
        child.transform.SetParent( gameObject.transform);
       

        lineRenderer = child.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.endColor = Color.white;
        lineRenderer.material = lineMat;


        Vector3 vec = gameObject.GetComponent<RectTransform>().anchoredPosition;
        //Vector3 vec = this.transform.TransformPoint(gameObject.GetComponent<RectTransform>().position);
        vec.x += gameObject.GetComponent<RectTransform>().rect.width;
        vec.y += gameObject.GetComponent<RectTransform>().rect.height /2;
        Vector3 vec2 = Camera.main.ScreenToWorldPoint(vec);
       // Debug.Log("vec="+vec);
       // Debug.Log("vec2="+vec2);
        Vector3 vec3 = targetOb.GetComponent<RectTransform>().anchoredPosition;
       // vec3.x += targetOb.GetComponent<RectTransform>().rect.width / 4;
        vec3.y += targetOb.GetComponent<RectTransform>().rect.height / 2;
        Vector3 vec4 = Camera.main.ScreenToWorldPoint(vec3);

        vec2.z = 0;
        vec4.z = 0;

        lineRenderer.SetPosition(0, vec2);
        lineRenderer.SetPosition(1, vec4);

        Debug.Log("vec=" + vec);
        Debug.Log("vec2=" + vec2);
        Debug.Log("vec3=" + vec3);
        Debug.Log("vec4=" + vec4);
    }
}
