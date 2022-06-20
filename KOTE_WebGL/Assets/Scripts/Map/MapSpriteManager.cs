using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace map
{
    public class MapSpriteManager : MonoBehaviour
    {
        public GameObject mapContainer;

        public NodeData nodePrefab;

        List<NodeData> nodes = new List<NodeData>();

        private bool royal_houses_mode_on = false;

        public GameObject playerIcon;
        public ParticleSystem portalAnimation;
        public GameObject nodesHolder;

        public GameObject LeftButton;
        public GameObject RightScrollButton;

        private float scrollSpeed;

        public Bounds mapBounds;

        private bool scrollMap;

        private Tween activeTween;

        // keep track of this so the map doesn't move if it's not bigger than the visible area
        private float halfScreenWidth;


        void Start()
        {
            GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener(OnMapNodesDataUpdated);
            GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.AddListener(OnToggleMap);
            GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
            GameManager.Instance.EVENT_MAP_SCROLL_CLICK.AddListener(OnScrollButtonClicked);
            GameManager.Instance.EVENT_MAP_SCROLL_DRAG.AddListener(OnMapScrollDragged);
            GameManager.Instance.EVENT_MAP_MASK_DOUBLECLICK.AddListener(OnMaskDoubleClick);
            GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.AddListener(OnPortalActivated);
            GameManager.Instance.EVENT_MAP_REVEAL.AddListener(OnRevealMap);

            playerIcon.SetActive(false);

            // we need half the width of the screen for various checks
            halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;
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

            //limit left scroll
            if (newPos.x < -mapBounds.max.x) newPos.x = -mapBounds.max.x;

            //limit the map move to the right
            if (newPos.x > 0 || mapBounds.max.x < halfScreenWidth * 2) newPos.x = 0;

            if (newPos.x < 0)
            {
                nodesHolder.transform.localPosition = Vector3.SmoothDamp(currentMapPos, newPos, ref velocity, 0.03f);
            }
            else
            {
                scrollSpeed = GameSettings.MAP_SCROLL_SPEED;
            }
        }


        private void OnScrollButtonClicked(bool active, bool direction)
        {
            scrollMap = active;

            if (active)
            {
                KillActiveTween();
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
            KillActiveTween();

            // make sure this script isn't scrolling
            scrollSpeed = 0;
            // and keep the map in bounds

            Vector3 newPos = nodesHolder.transform.localPosition;
            newPos.x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - dragOffset.x;
            newPos = transform.InverseTransformPoint(newPos);
            newPos.z = 0;

            //limit left scroll
            if (newPos.x < -mapBounds.max.x) newPos.x = -mapBounds.max.x;

            //limit the map move to the right
            if (newPos.x > 0 || mapBounds.max.x < halfScreenWidth * 2) newPos.x = 0;

            nodesHolder.transform.localPosition = newPos;
        }

        private void OnMapIconClicked()
        {
            //make sure when the map is on panel mode the nodes are not clickable
            foreach (NodeData node in nodes)
            {
                node.nodeClickDisabled = true;
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

        private void OnMapNodesDataUpdated(SWSM_MapData mapData)
        {
            GenerateMap(mapData);
            bool doBossScroll = true;
            foreach (NodeData node in nodes.FindAll(
                         x => x.type == NODE_TYPES.royal_house || x.type == NODE_TYPES.portal))
            {
                if (node.status == NODE_STATUS.active)
                {
                    doBossScroll = false;
                }
            }

            if (doBossScroll && GetBossNode() != null)
            {
               ScrollFromBoss();
                return;
            }

            ScrollBackToPlayerIcon();
        }

        //we will get to this point once the backend give us the node data
        void GenerateMap(SWSM_MapData expeditionMapData)
        {
            Debug.Log("[OnMapNodesDataUpdated] " + expeditionMapData);

            ClearMap();

            MapStructure mapStructure = GenerateMapStructure(expeditionMapData);

            InstantiateMapNodes(mapStructure);

            CreateNodeConnections();

            //at this point the map is completed. 
            //we get the maps bounds to help later with scroll limits and animations
            CalculateLocalBounds();
        }

        #region generateMap

        private void ClearMap()
        {
            while (nodes.Count > 0)
            {
                NodeData node = nodes[0];
                nodes.Remove(node);
                Destroy(node.gameObject);
            }
        }

        MapStructure GenerateMapStructure(SWSM_MapData expeditionMapData)
        {
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
            }

            return mapStructure;
        }


        void InstantiateMapNodes(MapStructure mapStructure)
        {
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

                        NodeData newNode = Instantiate(nodePrefab, nodesHolder.transform);
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

                        // if the node is active or the last completed node, move the player icon there
                        if (nodeData.status == NODE_STATUS.active.ToString() ||
                            nodeData.status == NODE_STATUS.completed.ToString())
                        {
                            playerIcon.SetActive(true);
                            playerIcon.transform.localPosition = newNode.transform.localPosition;
                        }

                        // if the node is an available royal house, turn royal house mode on
                        if (nodeData.status == NODE_STATUS.available.ToString() &&
                            nodeData.type == NODE_TYPES.royal_house.ToString()) royal_houses_mode_on = true;
                    }

                    //move next step (vertical group of nodes)
                    columnOffsetCounter += columnIncrement;
                }
            }
        }

        void CreateNodeConnections()
        {
            foreach (NodeData curNode in nodes)
            {
                //Debug.Log("Searching :" + go.GetComponent<NodeData>().id);

                foreach (int exitId in curNode.GetComponent<NodeData>().exits)
                {
                    NodeData exitNode = nodes.Find(x => x.id == exitId);

                    //if we find an exit node this becomes the target gameobject for the path sprite shape, and the exit 
                    // node for keeping track of the status
                    if (exitNode)
                    {
                        //go.GetComponent<NodeData>().UpdateLine(targetOb);
                        curNode.GetComponent<NodeData>().CreateSpriteShape(exitNode);
                    }
                    else
                    {
                        Destroy(curNode
                            .GetComponent<
                                LineRenderer>()); //as we are not longet using sprite renderer maybe we can remove this line
                        curNode.GetComponent<NodeData>().CreateSpriteShape(null);
                    }
                }
            }
        }

        #endregion

        void ScrollBackToPlayerIcon(float scrollTime = GameSettings.MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON)
        {
            float targetx = playerIcon.transform.localPosition.x / -2;

            Debug.Log("node:" + nodesHolder.transform.localPosition.x);
            Debug.Log("targetx:" + targetx);

            if ((mapBounds.max.x < halfScreenWidth * 2) == false)
            {
                activeTween = nodesHolder.transform.DOLocalMoveX(targetx, scrollTime);
            }
        }

        private void OnRevealMap(SWSM_MapData mapData)
        {
            // update the map as usual
            GenerateMap(mapData);

            // then hide the nodes and scroll towards the end
            foreach (NodeData node in nodes)
            {
                node.HideNode();
            }

            // animate the map based on the act of the last node, which should be the new act
            GameManager.Instance.EVENT_MAP_ANIMATE_STEP.Invoke(nodes[nodes.Count - 1].act, 0);

            StartCoroutine(RevealMapThenReturnToPlayer());
        }

        private IEnumerator RevealMapThenReturnToPlayer()
        {
            // TODO figure out a better way of calculating the animation time
            activeTween = nodesHolder.transform.DOLocalMoveX(-mapBounds.max.x, 10);
            yield return activeTween.WaitForCompletion();
            ScrollBackToPlayerIcon(GameSettings.MAP_SCROLL_ANIMATION_DURATION);
        }

        void ScrollFromBoss()
        {
            float targetX = GetBossNode().transform.localPosition.x * -1;
            nodesHolder.transform.position = new Vector3(targetX, 0, 0);
            StartCoroutine(ScrollFromBossToPlayer());
        }

        private IEnumerator ScrollFromBossToPlayer()
        {
            float targetX = GetBossNode().transform.localPosition.x * -1;
            nodesHolder.transform.DOLocalMoveX(targetX, GameSettings.MAP_SCROLL_ANIMATION_DURATION);
            yield return new WaitForSeconds(1);
            ScrollBackToPlayerIcon(GameSettings.MAP_SCROLL_ANIMATION_DURATION);
        }

        // get the boss node so we can move to it
        private NodeData GetBossNode()
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].subType == NODE_SUBTYPES.combat_boss)
                {
                    return nodes[i];
                }
            }
            return null;
        }

        private void OnPortalActivated(SWSM_MapData mapData)
        {
            // the portal is always the last node when we receive the portal activate event
            int nodeId = mapData.data.data[mapData.data.data.Length - 1].id;

            // move the particle system to the correct portal
            NodeData exitNode = nodes.Find(x => x.id == nodeId);
            portalAnimation.transform.position = exitNode.transform.position;

            // set the animation duration to the one specified in GameSettings
            ParticleSystem.MainModule portalAnimationMain = portalAnimation.main;
            portalAnimationMain.duration = GameSettings.PORTAL_ACTIVATION_ANIMATION_TIME;
            portalAnimation.Play();
            //TODO play map expansion animation
        }


        void CalculateLocalBounds()
        {
            Quaternion currentRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Bounds bounds = new Bounds(this.transform.position, Vector3.zero);

            foreach (Renderer renderer in nodesHolder.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // the bounds puts the final node in the middle of the screen, but we want it at the right edge


            // but just that cuts off the last node, so we need the size of the node as well
            float nodeWidth = nodes[nodes.Count - 1].GetComponent<BoxCollider2D>().size.x;

            // and subtract it from the bounds, but add the node width so it doesn't get cut off
            float newBoundsX = (bounds.extents.x - halfScreenWidth + nodeWidth);

            bounds.extents = new Vector3(newBoundsX, bounds.extents.y, bounds.extents.z);

            this.transform.rotation = currentRotation;
            mapBounds = bounds;
        }

        // kills the active tween to allow for player override
        private void KillActiveTween()
        {
            if (activeTween != null && activeTween.active)
            {
                activeTween.Kill();
            }
        }
    }
}