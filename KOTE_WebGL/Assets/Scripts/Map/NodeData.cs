using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeData : MonoBehaviour
{ 
    public int act;
    public int step;
    public int id;
    public string type;
    public int[] exits;
    public int[] enter;
        
    public Material lineMat;

    public bool disabled = false;

    LineRenderer lineRenderer;
    private void Start()
    {
        
        //Debug.Log(lineRenderer);
    }

   /* public void OnNodeClick()
    {
        Debug.Log("Node clicked "+id);
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(id);
    }*/

    public void UpdateLine(GameObject targetOb)
    {
        //Debug.Log("Updating line!");

        GameObject child = new GameObject();
        
        child.transform.SetParent( gameObject.transform);
       

        lineRenderer = child.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.endColor = Color.white;
        lineRenderer.material = lineMat;

        Vector3 sourcePosition = this.gameObject.transform.position;
        Vector3 targetPosition = targetOb.transform.position;

        //make sure lines are behind nodes
        sourcePosition.z++;
        targetPosition.z++;

        lineRenderer.SetPosition(0, sourcePosition);
        lineRenderer.SetPosition(1, targetPosition);

                
    }

    private void OnMouseDown()
    {
        if (!disabled)
        {
            Debug.Log("click");
            GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(this.id);
        }       
       
    }
}
