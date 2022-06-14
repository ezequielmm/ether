using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MapSpriteManager : MonoBehaviour
{
    public GameObject mapContainer;

    public GameObject nodePrefab;

    List<GameObject> nodes = new List<GameObject>();

    private bool royal_houses_mode_on = false;

    public GameObject playerIcon;
    public GameObject nodesHolder;

    public GameObject LeftButton;
    public GameObject RightScrollButton;

    private float scrollSpeed;

    public Bounds mapBounds;

    private bool scrollMap;


    void Start()
    {
        GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(OnMapNodesDataUpdated);
        GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.AddListener(OnToggleMap);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.AddListener(OnScrollButtonClicked);
        GameManager.Instance.EVENT_MAP_SCROLL_DRAG.AddListener(OnMapScrollDragged);
        GameManager.Instance.EVENT_MAP_MASK_DOUBLECLICK.AddListener(OnMaskDoubleClick);

        playerIcon.SetActive(false);
    }

    private void OnToggleMap(bool data)
    {
        mapContainer.SetActive(data);
    }

    private void OnMaskDoubleClick()
    {
        ScrollBackToPlayerIcon();
    }

    private void Update()
    {
        if (!scrollMap)
        {
            scrollSpeed = Mathf.SmoothStep(scrollSpeed, 0, Time.fixedDeltaTime * 2);
            // Debug.Log("scrollSpeed:" + scrollSpeed);
        }

        if (Mathf.Abs(scrollSpeed) < 0.01f) scrollSpeed = 0;


        Vector3 velocity = Vector3.zero;
        Vector3 currentMapPos = nodesHolder.transform.localPosition;

        Vector3 newPos = nodesHolder.transform.localPosition;

        newPos.x += scrollSpeed;

        //limit the map move to the right
        if (newPos.x > 0) newPos.x = 0;


        //limit left scroll
        if (newPos.x < -mapBounds.max.x) newPos.x = -mapBounds.max.x;

        if (newPos.x < 0)
        {
            nodesHolder.transform.localPosition = Vector3.SmoothDamp(currentMapPos, newPos, ref velocity, 0.03f);
        }
        else
        {
            scrollSpeed = 0;
        }
    }


    private void OnScrollButtonClicked(bool active, bool direction)
    {
        scrollMap = active;

        if (active)
        {
            if (direction)
            {
                scrollSpeed = GameSettings.MAP_SCROLL_SPEED * -1; //TODO magic number
            }
            else
            {
                scrollSpeed = GameSettings.MAP_SCROLL_SPEED;
            }
        }
    }

    // called while the player is dragging the map
    private void OnMapScrollDragged(Vector3 dragOffset)
    {
        // make sure this script isn't scrolling
        scrollSpeed = 0;
        // and keep the map in bounds

        Vector3 newPos = nodesHolder.transform.localPosition;
        newPos.x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - dragOffset.x;
        newPos = transform.InverseTransformPoint(newPos);
        newPos.z = 0;


        //limit the map move to the right
        if (newPos.x > 0) newPos.x = 0;

        //limit left scroll
        if (newPos.x < -mapBounds.max.x) newPos.x = -mapBounds.max.x;
        nodesHolder.transform.localPosition = newPos;
    }

    private void OnMapIconClicked()
    {
        //make sure when the map is on panel mode the nodes are not clickable
        foreach (GameObject go in nodes)
        {
            go.GetComponent<NodeData>().nodeClickDisabled = true;
        }

        if (mapContainer.activeSelf)
        {
            mapContainer.SetActive(false);
            GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.Invoke(false);
        }
        else
        {
            mapContainer.SetActive(true);
            GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.Invoke(true);
        }
    }



    //we will get to this point once the backend give us the node data
    void OnMapNodesDataUpdated(string data)
    {
        Debug.Log("[OnMapNodesDataUpdated] " + data);

        //ExpeditionMapData expeditionMapData = JsonUtility.FromJson<ExpeditionMapData>("{\"data\":" + data + "}");
        //ExpeditionMapData expeditionMapData = JsonUtility.FromJson<ExpeditionMapData>(data);
        SWSM_MapData expeditionMapData = JsonUtility.FromJson<SWSM_MapData>(data);

        MapStructure mapStructure = new MapStructure();

        //parse nodes data
        for (int i = 0; i < expeditionMapData.data.data.Length; i++)
        {
            NodeDataHelper nodeData = expeditionMapData.data.data[i];

            //acts
            if (mapStructure.acts.Count == 0 || mapStructure.acts.Count < (nodeData.act + 1))
            {
                Act newAct = new Act();
                mapStructure.acts.Add(newAct);
            }

            //steps
            if (mapStructure.acts[nodeData.act].steps.Count == 0 ||
                mapStructure.acts[nodeData.act].steps.Count < (nodeData.step + 1))
            {
                Step newStep = new Step();
                mapStructure.acts[nodeData.act].steps.Add(newStep);
            }

            //add id
            mapStructure.acts[nodeData.act].steps[nodeData.step].nodesData.Add(nodeData);

            if (nodeData.status == NODE_STATUS.available.ToString() &&
                nodeData.type == NODE_TYPES.royal_house.ToString()) royal_houses_mode_on = true;

            //Debug.Log("royal_houses_mode_on = "+ royal_houses_mode_on);
        }

        float columnOffsetCounter = GameSettings.MAP_SPRITE_NODE_X_OFFSET;
        float columnIncrement = GameSettings.MAP_SPRITE_NODE_X_OFFSET;


        //generate the map
        foreach (Act act in mapStructure.acts)
        {
            //areas
            foreach (Step step in act.steps)
            {
                //columns
                float rows = step.nodesData.Count;
                float rowsMaxSpace = 8 / rows;
                //Debug.Log("rowsMaxSpace:" + rowsMaxSpace);                

                foreach (NodeDataHelper nodeData in step.nodesData)
                {
                    float yy = (rowsMaxSpace * step.nodesData.IndexOf(nodeData)) - ((rows - 1) * rowsMaxSpace) / 2;

                    GameObject newNode = Instantiate(nodePrefab, nodesHolder.transform);
                    nodes.Add(newNode);

                    if (nodeData.type == NODE_TYPES.royal_house.ToString())
                    {
                        columnIncrement = GameSettings.MAP_SPRITE_NODE_X_OFFSET_RH;
                    }
                    else
                    {
                        columnIncrement = GameSettings.MAP_SPRITE_NODE_X_OFFSET;
                    }

                    newNode.transform.localPosition =
                        new Vector3(columnOffsetCounter, yy, GameSettings.MAP_SPRITE_ELEMENTS_Z);
                    newNode.GetComponent<NodeData>().Populate(nodeData);

                    if (nodeData.status == NODE_STATUS.active.ToString() ||
                        nodeData.status.Equals(NODE_STATUS.completed))
                    {
                        playerIcon.SetActive(true);
                        playerIcon.transform.localPosition = newNode.transform.localPosition;
                    }
                }

                //move next step (vertical group of nodes)
                columnOffsetCounter += columnIncrement;
            }
        }

        foreach (GameObject curNode in nodes)
        {
            //Debug.Log("Searching :" + go.GetComponent<NodeData>().id);

            foreach (int exitId in curNode.GetComponent<NodeData>().exits)
            {
                NodeData exitNode = nodes.Find(x => x.GetComponent<NodeData>().id == exitId).GetComponent<NodeData>();

                //if we find an exit node this becomes the target gameobject for the path sprite shape, and the exit 
                // node for keeping track of the status
                if (exitNode)
                {
                    //go.GetComponent<NodeData>().UpdateLine(targetOb);
                    curNode.GetComponent<NodeData>().UpdateSpriteShape(exitNode);
                }
                else
                {
                    Destroy(curNode
                        .GetComponent<LineRenderer>()); //as we are not longet using sprite renderer maybe we can remove this line
                    curNode.GetComponent<NodeData>().UpdateSpriteShape(null);
                }
            }
        }

        //at this point the map is completed. 
        //we get the maps bounds to help later with scroll limits and animations

        mapBounds = CalculateLocalBounds();

        ScrollBackToPlayerIcon();
    }

    void ScrollBackToPlayerIcon()
    {
        float targetx = playerIcon.transform.localPosition.x / -2;

        Debug.Log("node:" + nodesHolder.transform.localPosition.x);
        Debug.Log("targetx:" + targetx);

        nodesHolder.transform.DOLocalMoveX(targetx, GameSettings.MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON);
    }
    
    Bounds CalculateLocalBounds()
    {
        Quaternion currentRotation = this.transform.rotation;
        this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds bounds = new Bounds(this.transform.position, Vector3.zero);

        foreach (Renderer renderer in nodesHolder.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // the bounds puts the final node in the middle of the screen, but we want it at the right edge
        // so we get half the width of the screen
        float halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        
        // but just that cuts off the last node, so we need the size of the node as well
        float nodeWidth = nodes[nodes.Count - 1].GetComponent<BoxCollider2D>().size.x / 2;

        // and subtract it from the bounds, but add the node width so it doesn't get cut off
        float newBoundsX = (bounds.extents.x - halfScreenWidth + nodeWidth);
        
        bounds.extents = new Vector3(newBoundsX, bounds.extents.y, bounds.extents.z);

        this.transform.rotation = currentRotation;
        return bounds;
    }
}