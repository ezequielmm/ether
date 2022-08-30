using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace map
{
    public class MapSpriteManager : MonoBehaviour
    {
        public GameObject mapContainer;

        public NodeData nodePrefab;

        List<NodeData> nodes = new List<NodeData>();

        // we need a list of the path spriteshapes to use with the background grid
        private List<PathManager> pathManagers = new List<PathManager>();

        private bool royal_houses_mode_on = false;

        public GameObject playerIcon;
        public ParticleSystem portalAnimation;
        public GameObject nodesHolder;

        // tilemap references
        public Tilemap MapGrid;
        public Tile[] grassTiles;
        public Tile[] mountainTiles;
        public Tile[] forestTiles;

        public Vector3Int startPoint;
        public int startX;
        public int startY;
        public int endX;
        public int endY;

        public GameObject LeftButton;
        public GameObject RightScrollButton;

        public Camera mapCamera;
        public RenderTexture mapRenderTexture;
        private List<GameObject> hiddenMapItems;

        private float scrollTime;

        public Bounds mapBounds;
        private Bounds maskBounds;

        private bool scrollMap;
        private bool scrollDirection;

        private Tween activeTween;

        // keep track of this so the map doesn't move if it's not bigger than the visible area
        private float halfScreenWidth;

        private void Awake()
        {
            hiddenMapItems = new List<GameObject>();
        }

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
            maskBounds = GetComponentInChildren<SpriteMask>().GetComponent<BoxCollider2D>().bounds;
            
            // the particleSystem's sorting layer has to be set manually, because the the settings in the component don't work
            portalAnimation.GetComponent<Renderer>().sortingLayerName = GameSettings.MAP_ELEMENTS_SORTING_LAYER_NAME;
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
            float currentScrollSpeed = 0;
            if (scrollMap)
            {
                currentScrollSpeed =
                    Mathf.SmoothStep(scrollDirection ? -GameSettings.MAP_SCROLL_SPEED : GameSettings.MAP_SCROLL_SPEED,
                        0, scrollTime / GameSettings.MAP_SCROLL_BUTTON_TIME);
                if (scrollTime >= GameSettings.MAP_SCROLL_BUTTON_TIME)
                {
                    scrollMap = false;
                    scrollTime = 0;
                }

                scrollTime += Time.deltaTime;
            }

            if (Mathf.Abs(currentScrollSpeed) < GameSettings.MAP_SCROLL_SPEED_CUTOFF)
            {
                currentScrollSpeed = 0;
                scrollMap = false;
            }

            Vector3 velocity = Vector3.zero;
            Vector3 currentMapPos = nodesHolder.transform.localPosition;

            Vector3 newPos = nodesHolder.transform.localPosition;


            if (scrollMap)
                newPos.x += currentScrollSpeed;

            Vector3 limitPos = currentMapPos;

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
                limitPos.x = -GameSettings.MAP_STRETCH_LIMIT - (mapBounds.extents.x + mapBounds.center.x);
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
                limitPos.x = GameSettings.MAP_STRETCH_LIMIT - (-mapBounds.extents.x + mapBounds.center.x);
                overHardLimit = true;
            }

            if (overEdge || scrollMap)
            {
                nodesHolder.transform.localPosition = Vector3.SmoothDamp(currentMapPos, newPos, ref velocity, 0.03f);
            }

            if (overHardLimit && !scrollMap)
            {
                nodesHolder.transform.localPosition = limitPos;
            }

            if (overEdge && mapBounds.extents.x == 0)
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
                scrollDirection = direction;
                scrollTime = 0;
            }
        }


        // called while the player is dragging the map
        private void OnMapScrollDragged(Vector3 dragOffset)
        {
            KillActiveTween();

            // make sure this script isn't scrolling
            scrollTime = 0;
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

            GenerateMapGrid();

            // Generate Map Images
            StartCoroutine(GenerateMapImages());
        }

        #region generateMap

        private IEnumerator GenerateMapImages() 
        {
            yield return new WaitForSeconds(0.1f);

            float height = 2f * mapCamera.orthographicSize;
            float width = height * mapCamera.aspect;

            //mapRenderTexture.width = mapCamera.pixelWidth;
            //mapRenderTexture.height = mapCamera.pixelHeight;

            yield return new WaitForSeconds(0.1f);

            Quaternion currentRotation = nodesHolder.transform.rotation;
            Vector3 currentPosition = nodesHolder.transform.position;
            nodesHolder.transform.position = Vector3.zero;
            nodesHolder.transform.rotation = Quaternion.identity;

            Bounds bounds = new Bounds(nodesHolder.transform.position, Vector3.zero);

            foreach (Renderer renderer in nodesHolder.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            nodesHolder.transform.rotation = currentRotation;
            nodesHolder.transform.position = currentPosition;

            int imageCount = (int)Mathf.Ceil(bounds.size.x / width);
            for (int i = 0; i < imageCount; i++) 
            {
                mapCamera.transform.position = new Vector3(nodesHolder.transform.position.x + (i * width), nodesHolder.transform.position.y, mapCamera.transform.position.z);
                yield return new WaitForSeconds(0.1f);
                var img = toTexture2D(mapRenderTexture);
                GameObject imgObj = new GameObject();
                imgObj.transform.position = new Vector3(nodesHolder.transform.position.x + (i * width), nodesHolder.transform.position.y, nodesHolder.transform.position.z - 15);
                imgObj.transform.SetParent(nodesHolder.transform);
                imgObj.name = $"MapPathImage({i})";
                imgObj.tag = "MapImage";
                var sprite = imgObj.AddComponent<SpriteRenderer>();
                
                sprite.sortingLayerName = "MapElements";
                sprite.sortingOrder = 1;
                sprite.sprite = Sprite.Create(img, new Rect(0,0, mapRenderTexture.width, mapRenderTexture.height), Vector2.one * 0.5f);
                sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                // Scaling
                float widthScale = width / sprite.bounds.size.x;
                float heightScale = height / sprite.bounds.size.y;

                sprite.transform.localScale = new Vector3(widthScale, heightScale, 1);
            }

            foreach (var gameObj in GameObject.FindGameObjectsWithTag("MapPath")) 
            {
                gameObj.SetActive(false);
                hiddenMapItems.Add(gameObj);
            }
        }

        Texture2D toTexture2D(RenderTexture rTex)
        {
            RenderTexture.active = rTex;
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            // ReadPixels looks at the active RenderTexture.
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            return tex;
        }

        private void ClearMap()
        {
            foreach (var gameObj in hiddenMapItems)
            {
                gameObj.SetActive(true);
            }
            hiddenMapItems.Clear();

            while (nodes.Count > 0)
            {
                NodeData node = nodes[0];
                nodes.Remove(node);
                Destroy(node.gameObject);
            }

            foreach (var gameObj in GameObject.FindGameObjectsWithTag("MapImage"))
            {
                Destroy(gameObj);
            }

            // clear the references to the map paths
            pathManagers.Clear();
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
                            GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_TEXT.Invoke(nodeData.act, nodeData.step);
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
                        PathManager path = curNode.GetComponent<NodeData>().CreateSpriteShape(exitNode);
                        if (path != null) pathManagers.Add(path);
                    }
                    else
                    {
                        Destroy(curNode
                            .GetComponent<
                                LineRenderer>()); //as we are not longet using sprite renderer maybe we can remove this line
                        PathManager path = curNode.GetComponent<NodeData>().CreateSpriteShape(null);
                        if (path != null) pathManagers.Add(path);
                    }
                }
            }
        }

        private void GenerateMapGrid()
        {
            GeneratePathBackground();
            GenerateMapBackground();
        }

        private void GenerateMapBackground()
        {
            //in order for the grid to populate properly, we have to use SetTile, as BoxFill doesn't resize the grid
            // calculate the horizontal bounds of the grid first
            int gridStart = 0 - (int)halfScreenWidth;
            int gridEnd = (int)(mapBounds.max.x + halfScreenWidth * 2);

            // the vertical bounds of the map grid can be constant, as that's not going to change
            for (int height = -6; height < 6; height++)
            {
                for (int width = gridStart; width < gridEnd; width++)
                {
                    // randomly pick a tile type
                    int randomType = Random.Range(0, 2);
                    // we have to set the z to a constant, as for some reason you can two tiles in the same spot with different z levels
                    Vector3Int tilePos = new Vector3Int(width, height, (int)GameSettings.MAP_SPRITE_ELEMENTS_Z);
                    if (MapGrid.HasTile(tilePos) == false)
                    {
                        // pick a random tile of whatever type was selected
                        if (randomType == 0)
                        {
                            int randomTile = Random.Range(0, mountainTiles.Length);
                            MapGrid.SetTile(tilePos, mountainTiles[randomTile]);
                        }

                        if (randomType == 1)
                        {
                            int randomTile = Random.Range(0, forestTiles.Length);
                            MapGrid.SetTile(tilePos, forestTiles[randomTile]);
                        }
                    }
                }
            }
        }

        private void GeneratePathBackground()
        {
            //Generate the grass around the path
            if (pathManagers.Count > 0)
            {
                foreach (PathManager path in pathManagers)
                {
                    // get the references we need to generate the paths on the grid
                    SpriteShapeController pathSpriteController = path.pathController;
                    Spline pathSpline = pathSpriteController.spline;
                    int splinePoints = pathSpriteController.spline.GetPointCount();
                    Transform pathTransform = pathSpriteController.transform;

                    // for each of the points in the spline
                    for (int i = 0; i < splinePoints; i++)
                    {
                        // mark the grid tile underneath it as part of the path
                        Vector3 pointPosition = pathSpline.GetPosition(i);
                        pointPosition = pathTransform.TransformPoint(pointPosition);
                        Vector3Int cellPosition = MapGrid.layoutGrid.WorldToCell(pointPosition);
                        
                        // we have to set the z to a constant, as for some reason you can two tiles in the same spot with different z levels
                        cellPosition = new Vector3Int(cellPosition.x, cellPosition.y,
                            (int)GameSettings.MAP_SPRITE_ELEMENTS_Z);

                        int randomTile = Random.Range(0, grassTiles.Length);
                        MapGrid.SetTile(cellPosition, grassTiles[randomTile]);
                        
                        // if it's not the last point on the spline, move towards the next point and mark the tiles as path
                        if (i != splinePoints - 1)
                        {
                            Vector3 nextPoint = pathTransform.TransformPoint(pathSpline.GetPosition(i + 1));
                            Vector3 nextTilePos = Vector3.MoveTowards(pointPosition, nextPoint, 0.1f);
                            
                            while (Vector3.Distance(nextTilePos, nextPoint) > 0.1f)
                            {
                                Vector3Int nextTilePosition = MapGrid.layoutGrid.WorldToCell(nextTilePos);
                                
                                // we have to set the z to a constant, as for some reason you can two tiles in the same spot with different z levels
                                nextTilePosition = new Vector3Int(nextTilePosition.x, nextTilePosition.y,
                                    (int)GameSettings.MAP_SPRITE_ELEMENTS_Z);
                                
                                // set a random grass tile at that position
                                randomTile = Random.Range(0, grassTiles.Length);
                                MapGrid.SetTile(nextTilePosition, grassTiles[randomTile]);
                                
                                nextTilePos = Vector3.MoveTowards(nextTilePos, nextPoint, 0.1f);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateNodeBackground()
        {
            foreach (NodeData node in nodes)
            {
                Vector3 nodePos = nodesHolder.transform.TransformPoint(node.transform.position);
                Vector3Int nodeCelPos = MapGrid.layoutGrid.WorldToCell(nodePos);
                MapGrid.SetTile(nodeCelPos, grassTiles[0]);
                Debug.Log("nodePos: " + nodePos + " nodeCelPos: " + MapGrid.CellToWorld(nodeCelPos));
                // now we need to know where the node is in relation to the cell
                Vector3 cellPosition = MapGrid.CellToWorld(nodeCelPos);
                // we need to determine if it's in the center of the cell
                if (cellPosition.y - 0.2f < nodePos.y && nodePos.y < cellPosition.y + 0.2f)
                {
                    if (nodePos.x < cellPosition.x)
                    {
                        // node is on the left side, so color relevant tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x - 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x - 1, nodeCelPos.y + 1, nodeCelPos.z),
                            grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x - 1, nodeCelPos.y - 1, nodeCelPos.z),
                            grassTiles[0]);
                    }

                    if (nodePos.x > cellPosition.x)
                    {
                        // node is on the right side, so color those tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y + 1, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y - 1, nodeCelPos.z), grassTiles[0]);
                    }
                    else
                    {
                        // node is in the center, so color those tiles
                    }
                }

                // or if it's in the top section
                if (nodePos.y > cellPosition.y + 0.2f)
                {
                    if (nodePos.x <= cellPosition.x)
                    {
                        // node is on the left side, so color relevant tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x - 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y + 1, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y + 1, nodeCelPos.z),
                            grassTiles[0]);
                    }

                    if (nodePos.x > cellPosition.x)
                    {
                        // node is on the right side, so color those tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y + 1, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y + 1, nodeCelPos.z),
                            grassTiles[0]);
                    }
                }

                // or if it's in the bottom section
                if (nodePos.y < cellPosition.y - 0.2f)
                {
                    if (nodePos.x <= cellPosition.x)
                    {
                        // node is on the left side, so color relevant tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x - 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y - 1, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y - 1, nodeCelPos.z),
                            grassTiles[0]);
                    }

                    if (nodePos.x > cellPosition.x)
                    {
                        // node is on the right side, so color those tiles
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x, nodeCelPos.y - 1, nodeCelPos.z), grassTiles[0]);
                        MapGrid.SetTile(new Vector3Int(nodeCelPos.x + 1, nodeCelPos.y - 1, nodeCelPos.z),
                            grassTiles[0]);
                    }
                }
            }
        }

        #endregion

        void ScrollBackToPlayerIcon(float scrollTime = GameSettings.MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON,
            float knightPositionOnScreen = GameSettings.KNIGHT_SCREEN_POSITION_ON_CENTER)
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
            StartCoroutine(RevealMapThenReturnToPlayer(nodesHolder.transform.localPosition,
                GameSettings.MAP_SCROLL_ANIMATION_DURATION));
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
            scrollTime = 0;
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

            // if small map
            if (bounds.max.x < halfScreenWidth * 2)
            {
                bounds.extents = new Vector3(0, bounds.extents.y, bounds.extents.z);
            }
            else
            {
                //  Sets Left Edge

                // need to remove left side bounds to keep houses on left edge
                float leftEdge = Mathf.Abs(-maskBounds.extents.x + maskBounds.center.x) *
                                 GameSettings.MAP_LEFT_EDGE_MULTIPLIER;
                // Now we shrink and shift our bounds to the right
                // subtract half of left edge from extents
                bounds.extents = new Vector3(bounds.extents.x - (leftEdge / 2), bounds.extents.y, bounds.extents.z);
                // add half of the left edge to center
                bounds.center = new Vector3(bounds.center.x + (leftEdge / 2), bounds.center.y, bounds.center.z);

                // Sets Right Edge

                float rightEdge = Mathf.Abs(maskBounds.extents.x + maskBounds.center.x) *
                                  GameSettings.MAP_RIGHT_EDGE_MULTIPLIER;
                // Now we shrink and shift our bounds to the left
                // subtract half of right edge from extents
                bounds.extents = new Vector3(bounds.extents.x - (rightEdge / 2), bounds.extents.y, bounds.extents.z);
                // subtract half of the right edge to center
                bounds.center = new Vector3(bounds.center.x - (rightEdge / 2), bounds.center.y, bounds.center.z);
            }

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
                GizmoDrawBox(mapBounds, nodesHolder.transform.position);
            }

            if (maskBounds != null)
            {
                // Shows where the mask is
                Gizmos.color = Color.magenta;
                GizmoDrawBox(maskBounds);
            }
        }

        private void GizmoDrawBox(Bounds bounds, Vector2 offset = default(Vector2))
        {
            offset += (Vector2)bounds.center;
            GizmoDrawBox(
                new Vector2(bounds.extents.x + offset.x, bounds.extents.y + offset.y),
                new Vector2(-bounds.extents.x + offset.x, bounds.extents.y + offset.y),
                new Vector2(bounds.extents.x + offset.x, -bounds.extents.y + offset.y),
                new Vector2(-bounds.extents.x + offset.x, -bounds.extents.y + offset.y));
        }

        private void GizmoDrawBox(Vector2 TopLeft, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomRight)
        {
            Gizmos.DrawLine(TopLeft, TopRight);
            Gizmos.DrawLine(TopRight, BottomRight);
            Gizmos.DrawLine(BottomRight, BottomLeft);
            Gizmos.DrawLine(BottomLeft, TopLeft);
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