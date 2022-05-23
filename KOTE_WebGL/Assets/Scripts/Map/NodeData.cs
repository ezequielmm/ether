using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

[Serializable]
public class NodeData : MonoBehaviour  
{
    [Serializable]
    public struct BackgroundImage
    {
        public NODE_TYPES type;
        public GameObject imageGo;
    }

    [Header("Background sprites")]
    public List<BackgroundImage> bgSprites = new List<BackgroundImage>();

    public int act;
    public int step;
    public int id;
    public string type;
    public string status;
    public int[] exits;
    public int[] enter;
        
    public Material lineMat;

    public bool nodeClickDisabled = false;

    LineRenderer lineRenderer;

    public GameObject spriteShapePrefab;
    private GameObject spriteShape;
   

    private void Awake()
    {
        //Debug.Log("Node data awake");

        foreach (BackgroundImage bgimg in bgSprites)
        {
            bgimg.imageGo.SetActive(false);
        }
    }
    private void Start()
    {
       
        
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
        lineRenderer.sortingLayerName = GameSettings.MAP_ELEMENTS_SORTING_LAYER_NAME;
        lineRenderer.sortingOrder = GameSettings.MAP_LINE_RENDERER_SORTING_LAYER_ORDER;

        Vector3 sourcePosition = this.gameObject.transform.position;
        Vector3 targetPosition = targetOb.transform.position;

        //make sure lines are behind nodes
        sourcePosition.z++;
        targetPosition.z++;

        lineRenderer.SetPosition(0, sourcePosition);
        lineRenderer.SetPosition(1, targetPosition);
                
    }

    public void UpdateSpriteShape(GameObject targetOb,int exitID)
    {
        if(targetOb != null)
        {
            spriteShape = Instantiate(spriteShapePrefab, this.transform);
            // spriteShape.GetComponent<SpriteShapeController>().spline.SetPosition(4, spriteShape.transform.InverseTransformPoint(targetOb.transform.position));
            spriteShape.GetComponent<DottedLine>().Populate(targetOb, exitID,status);
        }
       
    }

    public void Populate(NodeDataHelper nodeData)
    {
        this.status = nodeData.status;
        this.id = nodeData.id;
        this.type = nodeData.type;
        this.exits = nodeData.exits;

        this.name = nodeData.type + "_"+nodeData.id;
                
        GetComponentInChildren<TextMeshPro>().SetText(nodeData.id.ToString());


        //Debug.Log("Node data populate");

        BackgroundImage bgi = bgSprites.Find(x => x.type == (NODE_TYPES)Enum.Parse(typeof(NODE_TYPES), nodeData.type));
        if (bgi.imageGo !=null )
        {
            bgi.imageGo.SetActive(true);
        }
        else
        {
            Debug.Log(" nodeData.type " + nodeData.type + " not found ");
        }

        

        Color indexColor = Color.grey;

        switch (Enum.Parse(typeof(NODE_STATUS),nodeData.status))
        {
            case NODE_STATUS.disabled:
                this.nodeClickDisabled = true; 
                break;
            case NODE_STATUS.completed:
                indexColor = Color.red;
                this.nodeClickDisabled = true; 
                break;
            case NODE_STATUS.active:
                if(nodeData.type == NODE_TYPES.portal.ToString()) this.nodeClickDisabled = true;
                indexColor = Color.cyan;
                break;  
            case NODE_STATUS.available:
                indexColor = Color.green;
                break;
        }

        GetComponentInChildren<TextMeshPro>().color = indexColor;
    }

    private void OnMouseDown()
    {
        if (!nodeClickDisabled)
        {
            Debug.Log("click");
            GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(this.id);
        }       
       
    }

    private void OnMouseOver()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.Invoke(id);
    }

    private void OnMouseExit()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.Invoke(-1);
    }


}
