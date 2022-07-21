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

        public Transform MapLeftEdge;

        private float scrollSpeed;

        public Bounds mapBounds;

        private bool scrollMap;

        private Tween activeTween;

        // keep track of this so the map doesn't move if it's not bigger than the visible area
        private float halfScreenWidth;


        void Start()
        {
            GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener(OnMapNodesDataUpdated);
            GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener(OnToggleMap);
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
            if (scrollMap)
            {
                scrollSpeed = Mathf.SmoothStep(scrollSpeed, 0, Time.fixedDeltaTime * GameSettings.MAP_SCROLL_BUTTON_TIME_MULTIPLIER);
                //Debug.Log($"[Map] scrollSpeed: {scrollSpeed}");
            }

            if (Mathf.Abs(scrollSpeed) < 0.01f) 
            {
                scrollSpeed = 0;
                scrollMap = false;
            }

            Vector3 velocity = Vector3.zero;
            Vector3 currentMapPos = nodesHolder.transform.localPosition;

            Vector3 newPos = nodesHolder.transform.localPosition;

            if(scrollMap)
                newPos.x += scrollSpeed;

            bool overEdge = false;
            bool overHardLimit = false;
            // limit scroll on the right (Boss Side)
            float rightEdge = mapBounds.extents.x + mapBounds.center.x + newPos.x;
            if (rightEdge < 0)
            {
                newPos.x = 0 - (mapBounds.extents.x + mapBounds.center.x);
                overEdge = true;
            }
            if (rightEdge < -GameSettings.MAP_STRETCH_LIMIT + -0.01f)
            {
                newPos.x = -GameSettings.MAP_STRETCH_LIMIT - (mapBounds.extents.x + mapBounds.center.x);
                overHardLimit = true;
                //Debug.Log($"[Map] Right Pass Bounds [{rightEdge} -/- {newPos.x}]");
            }

            // limit the map move on the Left (House Side)
            float leftEdge = -mapBounds.extents.x + mapBounds.center.x + newPos.x;
            if (leftEdge > 0)
            {
                newPos.x = 0 - (-mapBounds.extents.x + mapBounds.center.x);
                overEdge = true;
                //Debug.Log($"[Map] Left Pass Bounds [{leftEdge} -/- {newPos.x}]");
            }
            if (leftEdge > GameSettings.MAP_STRETCH_LIMIT + 0.01f)
            {
                newPos.x = GameSettings.MAP_STRETCH_LIMIT - (-mapBounds.extents.x + mapBounds.center.x);
                overHardLimit = true;
            }

            if (overEdge || scrollMap)
            {
                nodesHolder.transform.localPosition = Vector3.SmoothDamp(currentMapPos, newPos, ref velocity, 0.03f);
            }
            if (overHardLimit) 
            {
                nodesHolder.transform.localPosition = newPos;
            }
        }


        private void OnScrollButtonClicked(bool active, bool direction)
        {
            if (active)
            {
                scrollMap = true;
                KillActiveTween();
                if (direction)
                {
                    scrollSpeed = -GameSettings.MAP_SCROLL_SPEED;
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
                GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
            }
            else
            {
                mapContainer.SetActive(true);
                GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(true);
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

            Debug.Log("last node position: " + nodes[nodes.Count - 1].transform.position + " last node localPosition" +
                      nodes[nodes.Count - 1].transform.localPosition);
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
            float columnOffsetCounter = 0;
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

        void ScrollBackToPlayerIcon(float scrollTime = GameSettings.MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON, float knightPositionOnScreen = GameSettings.KNIGHT_SCREEN_POSITION_ON_CENTER)
        {
            // Put knight to center
            // Distance between knight and node origin.
            float disToKnight = playerIcon.transform.position.x - nodesHolder.transform.position.x;
            // Subtract desired knight position by distance to get node position.
            float targetx = (halfScreenWidth * knightPositionOnScreen) - disToKnight;


            if ((mapBounds.max.x < halfScreenWidth * 2) == false)
            {
                activeTween = nodesHolder.transform.DOLocalMoveX(targetx, scrollTime);
            }
        }

        private void OnRevealMap(SWSM_MapData mapData)
        {
            // update the map as usual
            GenerateMap(mapData);

            // animate the map based on the act of the last node, which should be the new act
            //TODO hopefully find a way of doing this that takes less processing time, currently too much for webgl
            /*
            int curAct = nodes[nodes.Count - 1].act;
           
            HideAllNodesInAct(curAct);
            GameManager.Instance.EVENT_MAP_ANIMATE_STEP.Invoke(curAct, 0);

            float numberOfSteps = nodes[nodes.Count - 1].step;

            StartCoroutine(RevealMapThenReturnToPlayer(nodesHolder.transform.localPosition, numberOfSteps));
            */
            StartCoroutine(RevealMapThenReturnToPlayer(nodesHolder.transform.localPosition, GameSettings.MAP_SCROLL_ANIMATION_DURATION));
        }

        private IEnumerator RevealMapThenReturnToPlayer(Vector3 mapPos, float animDuration)
        {
            yield return new WaitForSeconds(1); // Wait for map to load
            activeTween = nodesHolder.transform.DOLocalMoveX(-mapBounds.max.x, animDuration);
            yield return activeTween.WaitForCompletion();

            nodesHolder.transform.localPosition = new Vector3(-mapBounds.max.x, 0, 0);
            yield return new WaitForSeconds(2); // pause before return
            ScrollBackToPlayerIcon(GameSettings.MAP_SCROLL_ANIMATION_DURATION);
        }


        void ScrollFromBoss()
        {
            scrollSpeed = 0;
            nodesHolder.transform.localPosition = new Vector3(-mapBounds.max.x, 0, 0);
            StartCoroutine(ScrollFromBossToPlayer());
        }

        private IEnumerator ScrollFromBossToPlayer()
        {
            yield return new WaitForSeconds(1); // pause for loading
            ScrollBackToPlayerIcon(GameSettings.MAP_SCROLL_ANIMATION_DURATION, 0);
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

        private void HideAllNodesInAct(int act)
        {
            foreach (NodeData node in nodes)
            {
                if (node.act == act)
                {
                    node.HideNode();
                }
            }
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
            Quaternion currentRotation = nodesHolder.transform.rotation;
            Vector3 currentPosition = nodesHolder.transform.position;
            nodesHolder.transform.position = Vector3.zero;
            nodesHolder.transform.rotation = Quaternion.identity;

            Bounds bounds = new Bounds(nodesHolder.transform.position, Vector3.zero);
            
            foreach (Renderer renderer in nodesHolder.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // need to remove left side bounds to keep houses on left edge
            float leftEdge = Mathf.Abs(MapLeftEdge.position.x);
            // Now we shrink and shift our bounds to the right
            // subtract half of left edge from extents
            bounds.extents = new Vector3(bounds.extents.x - (leftEdge / 2), bounds.extents.y, bounds.extents.z);
            // add half of the left edge to center
            bounds.center = new Vector3(bounds.center.x + (leftEdge / 2), bounds.center.y, bounds.center.z);

            nodesHolder.transform.rotation = currentRotation;
            nodesHolder.transform.position = currentPosition;
            mapBounds = bounds;
            Debug.Log("[Map] Map Bounds Recalculated.");
        }

        private void OnDrawGizmosSelected()
        {
            if (mapBounds != null) 
            {
                // highlights the bounds in editor for debugging
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    new Vector3(mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20),
                    new Vector3(-mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20));
                Gizmos.DrawLine(
                    new Vector3(mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, -mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20),
                    new Vector3(-mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, -mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(
                    new Vector3(mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20),
                    new Vector3(mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, -mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20));
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    new Vector3(-mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, -mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20),
                    new Vector3(-mapBounds.extents.x + mapBounds.center.x + nodesHolder.transform.position.x, mapBounds.extents.y + mapBounds.center.y + nodesHolder.transform.position.y, 20));
            }
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(
                new Vector3(MapLeftEdge.position.x, -10, 20),
                new Vector3(MapLeftEdge.position.x, 10, 20));
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