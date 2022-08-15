using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

[Serializable]
public class NodeData : MonoBehaviour
{
    [Serializable]
    public struct BackgroundImage
    {
        public NODE_SUBTYPES type;
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
    public TextMeshPro idText;


    public bool nodeClickDisabled = false;

    LineRenderer lineRenderer;

    public GameObject spriteShapePrefab;
    private GameObject spriteShape;

    #region UnityEventFunctions

    private void Awake()
    {
        HideNode();
    }


    private void OnMouseDown()
    {
        if (!nodeClickDisabled)
        {
            Debug.Log("click");
            // if clicking on a royal house node, we want to ask the player for confirmation before activating the node
            if (type == NODE_TYPES.royal_house)
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                    "Do you want to enter" + Utils.CapitalizeEveryWordOfEnum(subType), OnConfirmRoyalHouse);
                return;
            }
            else
            {
                GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(id);
                GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_TEXT.Invoke(act, step);
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

    public void HideNode()
    {
        foreach (BackgroundImage bgimg in bgSprites)
        {
            bgimg.imageGo.SetActive(false);
        }

        idText.gameObject.SetActive(false);
    }

    public void ShowNode()
    {
        SelectNodeImage();
        UpdateNodeStatusVisuals();
    }

    public void Populate(NodeDataHelper nodeData)
    {
        PopulateNodeInformation(nodeData);
        SelectNodeImage();
        UpdateNodeStatusVisuals();
    }

    private void PopulateNodeInformation(NodeDataHelper nodeData)
    {
        status = Utils.ParseEnum<NODE_STATUS>(nodeData.status);
        id = nodeData.id;
        type = Utils.ParseEnum<NODE_TYPES>(nodeData.type);
        subType = Utils.ParseEnum<NODE_SUBTYPES>(nodeData.subType);
        exits = nodeData.exits;
        name = nodeData.type + "_" + nodeData.id;
        act = nodeData.act;
        step = nodeData.step;
    }

    private void SelectNodeImage()
    {
        BackgroundImage bgi = bgSprites.Find(x => x.type == subType);
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
            Debug.Log(" nodeData.type " + type + " not found ");
        }
    }

    private void UpdateNodeStatusVisuals()
    {
        Color indexColor = Color.grey;

        switch (status)
        {
            case NODE_STATUS.disabled:
                nodeClickDisabled = true;
                break;
            case NODE_STATUS.completed:
                indexColor = Color.red;
                nodeClickDisabled = true;
                break;
            case NODE_STATUS.active:
                if (type == NODE_TYPES.portal) nodeClickDisabled = true;
                indexColor = Color.cyan;
                break;
            case NODE_STATUS.available:
                indexColor = Color.green;
                availableParticleSystem.Play();
                break;
        }

        idText.SetText(id.ToString());
        idText.color = indexColor;
        idText.gameObject.SetActive(true);
    }


    // when we update the sprite shape, we pass it the node data for the exit node directly, because it needs three things from it
    public PathManager CreateSpriteShape(NodeData exitNode)
    {
        if (exitNode != null)
        {
            spriteShape = Instantiate(spriteShapePrefab, this.transform);
            // spriteShape.GetComponent<SpriteShapeController>().spline.SetPosition(4, spriteShape.transform.InverseTransformPoint(targetOb.transform.position));
            PathManager path = spriteShape.GetComponent<PathManager>();
            path.Populate(exitNode, status);
            return path;
        }

        return null;
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