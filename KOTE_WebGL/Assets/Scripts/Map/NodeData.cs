using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class NodeData : MonoBehaviour
{
    [Serializable]
    public struct BackgroundImage
    {
        public NODE_TYPES type;
        public GameObject imageGo;
    }

    [Header("Background sprites")] public List<BackgroundImage> bgSprites = new List<BackgroundImage>();

    public int act;
    public int step;
    public int id;
    public NODE_TYPES type;
    public NODE_SUBTYPES subType;
    public NODE_STATUS status;
    public int[] exits;
    public int[] enter;

    public Material lineMat;
    public Material grayscaleMaterial;
    [FormerlySerializedAs("pSystem")] public ParticleSystem availableParticleSystem;
   

    public bool nodeClickDisabled = false;

    LineRenderer lineRenderer;

    public GameObject spriteShapePrefab;
    private GameObject spriteShape;

    #region UnityEventFunctions

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
        //GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.AddListener(ActivatePortal);
    }

    private void OnMouseDown()
    {
        if (!nodeClickDisabled)
        {
            Debug.Log("click");
            // if clicking on a royal house node, we want to ask the player for confirmation before activating the node
            if (type == NODE_TYPES.royal_house)
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Do you want to enter" + Utils.CapitalizeEveryWordOfEnum(subType), OnConfirmRoyalHouse);
                return;
            }
            else
            {
                GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(this.id);
            }

            
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

    #endregion
    public void Populate(NodeDataHelper nodeData)
    {
        PopulateNodeInformation(nodeData);
        SelectNodeImage(nodeData);
        UpdateNodeStatusVisuals(nodeData);
    }
    
    private void PopulateNodeInformation(NodeDataHelper nodeData)
    {
        status = (NODE_STATUS)Enum.Parse(typeof(NODE_STATUS), nodeData.status);
        id = nodeData.id;
        type = (NODE_TYPES)Enum.Parse(typeof(NODE_TYPES), nodeData.type);
        subType = (NODE_SUBTYPES)Enum.Parse(typeof(NODE_SUBTYPES), nodeData.subType);
        exits = nodeData.exits;
        name = nodeData.type + "_" + nodeData.id;
    }

    private void SelectNodeImage(NodeDataHelper nodeData)
    {
        BackgroundImage bgi = bgSprites.Find(x => x.type == type);
        if (bgi.imageGo != null)
        {
            bgi.imageGo.SetActive(true);
            if (status == NODE_STATUS.disabled)
            {
                bgi.imageGo.GetComponent<SpriteRenderer>().material = grayscaleMaterial;
            }
        }
        else
        {
            Debug.Log(" nodeData.type " + nodeData.type + " not found ");
        }
    }

    private void UpdateNodeStatusVisuals(NodeDataHelper nodeData)
    {
        Color indexColor = Color.grey;

        switch (Enum.Parse(typeof(NODE_STATUS), nodeData.status))
        {
            case NODE_STATUS.disabled:
                this.nodeClickDisabled = true;
                break;
            case NODE_STATUS.completed:
                indexColor = Color.red;
                this.nodeClickDisabled = true;
                break;
            case NODE_STATUS.active:
                if (nodeData.type == NODE_TYPES.portal.ToString()) this.nodeClickDisabled = true;
                indexColor = Color.cyan;
                break;
            case NODE_STATUS.available:
                indexColor = Color.green;
                availableParticleSystem.Play();
                break;
        }

        GetComponentInChildren<TextMeshPro>().SetText(nodeData.id.ToString());
        GetComponentInChildren<TextMeshPro>().color = indexColor;
    }


    // when we update the sprite shape, we pass it the node data for the exit node directly, because it needs three things from it
    public void UpdateSpriteShape(NodeData exitNode)
    {
        if (exitNode != null)
        {
            spriteShape = Instantiate(spriteShapePrefab, this.transform);
            // spriteShape.GetComponent<SpriteShapeController>().spline.SetPosition(4, spriteShape.transform.InverseTransformPoint(targetOb.transform.position));
            spriteShape.GetComponent<PathManager>().Populate(exitNode, status);
        }
    }

    private void OnConfirmRoyalHouse()
    {
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(this.id);        
    }  

    #region oldFunctions

    //TODO eventually get rid of this, this is from the old map generation
    public void UpdateLine(GameObject targetOb)
    {
        //Debug.Log("Updating line!");

        GameObject child = new GameObject();

        child.transform.SetParent(gameObject.transform);

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

    #endregion
}