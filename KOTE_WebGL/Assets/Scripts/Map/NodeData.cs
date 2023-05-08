using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

[Serializable]
public class NodeData : MonoBehaviour, ITooltipSetter
{
    [Serializable]
    public struct BackgroundImage
    {
        public NODE_SUBTYPES type;
        public NodeView NodeView;
    }

    [Serializable]
    public struct BossImage
    {
        public string bossName;
        public GameObject imageGo;
    }

    private NodeView myNodeView;
    
    [Header("Background sprites")] public List<BackgroundImage> bgSprites = new List<BackgroundImage>();
    public List<BossImage> bossSprites = new List<BossImage>();

    public int act;
    public int step;
    public int id;
    public NODE_TYPES type;
    public NODE_SUBTYPES subType;
    public string title;
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
    public GameObject NodeArt;
    private GameObject spriteShape;
    private GameObject activeIconImage;
    private Vector3 originalScale;
    private Tween activeAnimation;
    private bool showNodeNumber;
    private bool playHoverAnimation;
    private List<Tooltip> tooltips;

    #region UnityEventFunctions

    private void Awake()
    {
        // the particleSystem's sorting layer has to be set manually, because the the settings in the component don't work
        availableParticleSystem.GetComponent<Renderer>().sortingLayerName =
            GameSettings.MAP_ELEMENTS_SORTING_LAYER_NAME;
        HideNode();
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnShowMapPanel);
    }


    private void OnMouseDown()
    {
        if (nodeClickDisabled) return;
        
        Debug.Log("click");
        // if clicking on a royal house node, we want to ask the player for confirmation before activating the node
        if (type == NODE_TYPES.royal_house)
        {
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "Do you want to enter " + title + "?", OnConfirmRoyalHouse);
            return;
        }
        
        if (type == NODE_TYPES.combat)
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Combat Selected");
        }

        if (subType == NODE_SUBTYPES.combat_boss)
        {
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Boss, act);
        }

        GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(id);
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.Invoke(act, step);
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        StopActiveNodeAnimation();
    
    }

    private void OnMouseEnter()
    {
        SetTooltip(tooltips);
    }

    private void OnMouseOver()
    {
        myNodeView.OnHover();
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.Invoke(id);
    }

    private void OnMouseExit()
    {
        myNodeView.OnUnHover();
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.Invoke(-1);
    }

    #endregion

    public void HideNode()
    {
        foreach (BackgroundImage bgimg in bgSprites)
        {
            bgimg.NodeView.Hide();
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
        int numbersEnabled = PlayerPrefs.GetInt("enable_node_numbers");
        if (numbersEnabled == 1) showNodeNumber = true;
        PopulateNodeInformation(nodeData);
        PopulateTooltip(nodeData);
        SelectNodeImage();
        UpdateNodeStatusVisuals();
    }

    private void PopulateNodeInformation(NodeDataHelper nodeData)
    {
        status = nodeData.status.ParseToEnum<NODE_STATUS>();
        id = nodeData.id;
        type = nodeData.type.ParseToEnum<NODE_TYPES>();
        subType = nodeData.subType.ParseToEnum<NODE_SUBTYPES>();
        exits = nodeData.exits;
        name = nodeData.type + "_" + nodeData.id;
        act = nodeData.act;
        step = nodeData.step;
        title = nodeData.title;
    }

    private void PopulateTooltip(NodeDataHelper nodeData)
    {
        Tooltip tooltip = new Tooltip();
        tooltip.title = FormatTooltip(nodeData.title);
        tooltips = new List<Tooltip> { tooltip };
    }

    private string FormatTooltip(string tooltipDesc)
    {
        string[] split = tooltipDesc.Split('_');
        if (split.Length > 1) return Utils.PrettyText(split[1] + " " + split[0]);
        split = tooltipDesc.Split();
        if (split.Length > 1) return Utils.PrettyText(split[0] + split[1]);
        return Utils.PrettyText(split[0]);
    }

    private void SelectNodeImage()
    {
        myNodeView = bgSprites.Find(x => x.type == subType).NodeView;
        if (myNodeView == null)
        {
            Debug.Log(" nodeData.type " + type + " not found ");
            return;
        }

        myNodeView.Init(status);
        myNodeView.Show();

        // resize the node depending on the status
        myNodeView.SetResize();
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
                if (type == NODE_TYPES.portal)
                {
                    availableParticleSystem.Play();
                }

                break;
        }

        idText.SetText(id.ToString());
        idText.color = indexColor;
        idText.gameObject.SetActive(showNodeNumber);
    }

    private void PlayActiveNodeAnimation(GameObject backgroundImage)
    {
        activeAnimation = backgroundImage.transform.DOScale(backgroundImage.transform.localScale * 0.7f,
                GameSettings.ACTIVE_NODE_PULSE_TIME)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopActiveNodeAnimation()
    {
        myNodeView.Stop();
    }

    private void OnShowMapPanel()
    {
        myNodeView.Rewind();
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

    public void SetTooltip(List<Tooltip> tooltips)
    {
        Vector3 tooltipAnchor = transform.position;
        tooltipAnchor.y -= 0.5f;
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.TopCenter,
            tooltipAnchor, null);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}