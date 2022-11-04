using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
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

        [Tooltip("The range of strength of the spine's knots")] [Min(0f)]
        public Vector2 pathCurveStrengthRange;

        [Tooltip("The range of random angle fuzziness on each knot")]
        public Vector2 angleFuzziness;

        [Tooltip("The random position to add to a path")]
        public float pathPositionFuzziness;

        [Tooltip("The random distance between different column.")]
        public Vector2 columnFuzziness;

        public Camera mapCamera;
        private List<GameObject> hiddenMapItems;

        private float scrollTime;

        public Bounds mapBounds;
        private Bounds maskBounds;

        private bool scrollMap;
        private bool scrollDirection;

        private Tween activeTween;

        // keep track of this so the map doesn't move if it's not bigger than the visible area
        private float halfScreenWidth;

        private List<int> MapSeeds;
        private Dictionary<Vector3Int, MapTilePath> mapPaths;
        List<Vector3Int> blockedTiles = new List<Vector3Int>();
        Dictionary<Vector3Int, MapTilePath> allTiles = new Dictionary<Vector3Int, MapTilePath>();
        Dictionary<Vector3Int, TileSplineRef> tileSplineRef = new Dictionary<Vector3Int, TileSplineRef>();
        Dictionary<Vector3Int, GameObject> nodeMapRef = new Dictionary<Vector3Int, GameObject>();


        private class SplinePoint
        {
            public Vector3Int tileLoc;
            public Spline path;
            public Spline dash;
            public int index;
            public Transform pathTransform;

            public SplinePoint(Vector3Int TileAddress, Spline PathSpline, Spline DashSpline, int Index,
                Transform transform)
            {
                tileLoc = TileAddress;
                path = PathSpline;
                dash = DashSpline;
                index = Index;
                pathTransform = transform;
            }
        }

        private void OnValidate()
        {
            pathCurveStrengthRange = keepMinMax(pathCurveStrengthRange);
            angleFuzziness = keepInRange(keepMinMax(angleFuzziness), -360, 360);
            columnFuzziness = keepInRange(keepMinMax(columnFuzziness), 0, GameSettings.MAP_SPRITE_NODE_X_OFFSET);
        }

        private Vector2 keepInRange(Vector2 value, float min, float max)
        {
            value.x = Mathf.Clamp(value.x, min, max);
            value.y = Mathf.Clamp(value.y, min, max);
            return value;
        }

        private Vector2 keepMinMax(Vector2 value)
        {
            if (value.x > value.y)
            {
                value.y = value.x;
            }
            else if (value.y < value.x)
            {
                value.x = value.y;
            }

            return value;
        }

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
            GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnNodeSelected);

            playerIcon.SetActive(false);

            // we need half the width of the screen for various checks
            halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;
            maskBounds = GetComponentInChildren<SpriteMask>().GetComponent<BoxCollider2D>().bounds;

            // the particleSystem's sorting layer has to be set manually, because the the settings in the component don't work
            portalAnimation.GetComponent<Renderer>().sortingLayerName = GameSettings.MAP_ELEMENTS_SORTING_LAYER_NAME;

            GenerateMapSeeds(22);
        }

        private void GenerateMapSeeds(int seed) 
        {
            Random.InitState(seed);
            MapSeeds = new List<int>();
            for (int i = 0; i < 10; i++) 
            {
                MapSeeds.Add(Random.Range(0,100));
            }
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
                //Debug.Log($"[MapSpriteManager] Right Pass Bounds [{rightEdge} -/- {newPos.x}]");
            }

            // limit the map move on the Left (House Side)
            float leftEdge = -mapBounds.extents.x + mapBounds.center.x + newPos.x;
            if (leftEdge > 0)
            {
                newPos.x = 0 - (-mapBounds.extents.x + mapBounds.center.x);
                overEdge = true;
                //Debug.Log($"[MapSpriteManager] Left Pass Bounds [{leftEdge} -/- {newPos.x}]");
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

        private void OnNodeSelected(int id)
        {
            if (nodes.Exists(node => node.id == id))
            {
                NodeData curNode = nodes.Find(node => node.id == id);
                playerIcon.transform.localPosition = curNode.transform.localPosition;
            }
        }

        private void OnMapNodesDataUpdated(SWSM_MapData mapData)
        {
            GenerateMap(mapData);
            StartCoroutine(Scroll());
        }

        IEnumerator Scroll()
        {
            yield return new WaitForSeconds(0.5f);
            ScrollBackToPlayerIcon();
        }

        //we will get to this point once the backend give us the node data
        void GenerateMap(SWSM_MapData expeditionMapData)
        {
            Debug.Log("[MapSpriteManager | OnMapNodesDataUpdated] " + expeditionMapData);

            ClearMap();

            // Set Seed
            GenerateMapSeeds(expeditionMapData.data.seed);
            Debug.Log($"[MapSpriteManager] Map Seed: {expeditionMapData.data.seed}");

            MapStructure mapStructure = GenerateMapStructure(expeditionMapData);

            Random.InitState(MapSeeds[4]);
            InstantiateMapNodes(mapStructure);

            CreateNodeConnections();

            //at this point the map is completed. 
            //we get the maps bounds to help later with scroll limits and animations
            CalculateLocalBounds();

            Debug.Log("[MapSpriteManager] last node position: " + nodes[nodes.Count - 1].transform.position + " last node localPosition" +
                      nodes[nodes.Count - 1].transform.localPosition);

            GenerateMapGrid();

            // Generate Map Images
            //StartCoroutine(GenerateMapImages());
        }

        #region generateMap

        private IEnumerator GenerateMapImages()
        {
            yield return new WaitForEndOfFrame();

            float height = 2f * mapCamera.orthographicSize;
            float originalWidth = height * mapCamera.aspect;
            int pixHeight = Mathf.Max(Screen.height, 1080);

            //mapRenderTexture.width = mapCamera.pixelWidth;
            //mapRenderTexture.height = mapCamera.pixelHeight;

            yield return new WaitForEndOfFrame();

            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);

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

            float width = bounds.size.x;
            mapCamera.aspect = (width + originalWidth) / height;

            mapCamera.transform.position = new Vector3(nodesHolder.transform.position.x + (width * 0.5f) - (originalWidth * 0.5f),
                nodesHolder.transform.position.y, mapCamera.transform.position.z);

            RenderTexture mapRenderTexture = new RenderTexture((int)((width + originalWidth) / height * pixHeight), pixHeight, 24);
            mapCamera.targetTexture = mapRenderTexture;

            yield return new WaitForEndOfFrame();
            // Create Map Texture

            var img = toTexture2D(mapRenderTexture);
            GameObject imgObj = new GameObject();
            imgObj.transform.position = new Vector3(nodesHolder.transform.position.x + (width * 0.5f) - (originalWidth * 0.5f),
                nodesHolder.transform.position.y, nodesHolder.transform.position.z - 15);
            imgObj.transform.SetParent(nodesHolder.transform);
            imgObj.name = $"MapPathImage";
            imgObj.tag = "MapImage";
            var sprite = imgObj.AddComponent<SpriteRenderer>();

            sprite.sortingLayerName = "MapElements";
            sprite.sortingOrder = 1;
            sprite.sprite = Sprite.Create(img, new Rect(0, 0, mapRenderTexture.width, mapRenderTexture.height),
                Vector2.one * 0.5f);
            sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            // Scaling
            float widthScale = (width + originalWidth) / sprite.bounds.size.x;
            float heightScale = height / sprite.bounds.size.y;

            sprite.transform.localScale = new Vector3(widthScale, heightScale, 1);

            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(false);

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

            blockedTiles.Clear();
            allTiles.Clear();
            tileSplineRef.Clear();
            nodeMapRef.Clear();
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
                    //Debug.Log("[MapSpriteManager] rowsMaxSpace:" + rowsMaxSpace);                

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

                        float columnRan = Random.Range(columnFuzziness.x, columnFuzziness.y);

                        // Set Cell's Position
                        newNode.transform.localPosition = new Vector3(columnOffsetCounter + columnRan, yy,
                            GameSettings.MAP_SPRITE_ELEMENTS_Z);

                        // Snap cell to grid
                        Vector3Int mapPos = MapGrid.layoutGrid.WorldToCell(newNode.transform.position);
                        mapPos.z = 0;
                        Vector3 finalPos = MapGrid.layoutGrid.CellToWorld(mapPos);
                        finalPos.z = GameSettings.MAP_SPRITE_ELEMENTS_Z;
                        newNode.transform.position = finalPos;

                        // Record node
                        nodeMapRef.Add(mapPos, newNode.NodeArt);


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
                //Debug.Log("[MapSpriteManager] Searching :" + go.GetComponent<NodeData>().id);

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
            Random.InitState(MapSeeds[0]);
            GeneratePathBackground();

            CurvePath();

            Random.InitState(MapSeeds[2]);
            GenerateMapBackground();
        }

        private void CurvePath()
        {
            int i = 0;
            foreach (var splineRef in tileSplineRef.Values)
            {
                Random.InitState(MapSeeds[1] + i++);
                RunRandomCurve(splineRef);
                splineRef.EnforceSplineMatch();
                if (nodeMapRef.ContainsKey(splineRef.Position)) 
                {
                    nodeMapRef[splineRef.Position].transform.position = splineRef.MasterSpline.Position;
                }
            }
        }

        private void RunRandomCurve(TileSplineRef splineRef) //SplineData splineData)
        {
            // Middle of Path can be main
            SplineData masterNode = splineRef.MasterSpline;

            if (splineRef.MasterRandomized) return;
            splineRef.MasterRandomized = true;

            // Set position
            Vector3 currentPos = masterNode.Position;
            Vector3 randomPos = new Vector3(Random.Range(-pathPositionFuzziness, pathPositionFuzziness),
                Random.Range(-pathPositionFuzziness, pathPositionFuzziness), 0);
            masterNode.Position = currentPos + randomPos;

            // Path
            if (!masterNode.IsEndPiece)
            {
                // Set angles
                float spineStrengthL = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                float spineStrengthR = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);

                float angleRangeL = Random.Range(angleFuzziness.x, angleFuzziness.y);
                float angleRangeR = Random.Range(angleFuzziness.x, angleFuzziness.y);

                Vector3 rightPoint =
                    masterNode.Transform.TransformPoint(masterNode.Spline.GetPosition(masterNode.Index + 1));
                Vector3 leftPoint =
                    masterNode.Transform.TransformPoint(masterNode.Spline.GetPosition(masterNode.Index - 1));

                Vector3 leftTangent = (leftPoint - masterNode.Position).normalized;
                Vector3 rightTangent = (rightPoint - masterNode.Position).normalized;

                Vector3 averageDir = (leftTangent + rightTangent).normalized;

                if (averageDir.magnitude > 0)
                {
                    float rotationAmount = 90;
                    float value = Vector3.Dot(Vector3.right, Quaternion.Euler(0, 0, rotationAmount) * averageDir);
                    if (value > 0)
                    {
                        rotationAmount = -rotationAmount;
                    }

                    leftTangent = Quaternion.Euler(0, 0, rotationAmount + angleRangeL) * averageDir;
                    rightTangent = Quaternion.Euler(0, 0, -rotationAmount + angleRangeR) * averageDir;
                }
                else
                {
                    leftTangent = Quaternion.Euler(0, 0, angleRangeL) * leftTangent;
                    rightTangent = Quaternion.Euler(0, 0, angleRangeR) * rightTangent;
                }

                masterNode.Spline.SetTangentMode(masterNode.Index, ShapeTangentMode.Continuous);
                masterNode.LeftTangent = leftTangent * spineStrengthL;
                masterNode.RightTangent = rightTangent * spineStrengthR;
            }

            // Dashed line
            foreach (var splineData in splineRef.ChildrenSplines)
            {
                if (splineData.IsEndPiece)
                {
                    // Set angles on end points
                    Vector3 leftTangent = Vector3.zero;
                    Vector3 rightTangent = Vector3.zero;
                    bool first = splineData.Index == 0;
                    if (first)
                    {
                        float angleRangeR = Random.Range(angleFuzziness.x, angleFuzziness.y);
                        Vector3 rightPoint =
                            splineData.Transform.TransformPoint(splineData.Spline.GetPosition(splineData.Index + 1));
                        float spineStrengthR = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                        rightTangent = Quaternion.Euler(0, 0, angleRangeR) *
                                       (rightPoint - splineData.Position).normalized * spineStrengthR;
                    }
                    else
                    {
                        float angleRangeL = Random.Range(angleFuzziness.x, angleFuzziness.y);
                        Vector3 leftPoint =
                            splineData.Transform.TransformPoint(splineData.Spline.GetPosition(splineData.Index - 1));
                        float spineStrengthL = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                        leftTangent = Quaternion.Euler(0, 0, angleRangeL) *
                                      (leftPoint - splineData.Position).normalized * spineStrengthL;
                    }

                    splineData.LeftTangent = leftTangent;
                    splineData.RightTangent = rightTangent;
                }
            }
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
                    Vector3Int
                        tilePos = GetVectorWithZ(width,
                            height); // new Vector3Int(width, height, (int)GameSettings.MAP_SPRITE_ELEMENTS_Z);
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
                    else 
                    {
                        // Keep the ransomness consistant
                        _ = Random.Range(0, 2);
                    }
                }
            }
        }

        private void GeneratePathBackground()
        {
            //Generate the grass around the path and set up the path in the process
            // Clear out path dictionary for generation
            if (mapPaths != null)
            {
                mapPaths.Clear();
            }
            else
            {
                mapPaths = new Dictionary<Vector3Int, MapTilePath>();
            }

            // Skip if no paths
            if (pathManagers.Count <= 0)
            {
                return;
            }

            // Set the paths up first
            foreach (PathManager path in pathManagers)
            {
                Spline spline = path.pathController.spline;
                for (int i = 1; i < spline.GetPointCount() - 1; i++)
                {
                    spline.RemovePointAt(i);
                }

                Vector3Int start = MapGrid.WorldToCell(path.transform.TransformPoint(spline.GetPosition(0)));
                Vector3Int end =
                    MapGrid.WorldToCell(path.transform.TransformPoint(spline.GetPosition(spline.GetPointCount() - 1)));
                SnapPath(start, end, path);
            }
        }

        private void SetNodeGrass(Vector3Int node)
        {
            // we have to set the z to a constant, as for some reason you can two tiles in the same spot with different z levels
            node = GetVectorWithZ(node.x, node.y);

            int randomTile = Random.Range(0, grassTiles.Length);
            MapGrid.SetTile(node, grassTiles[randomTile]);
        }


        // Snaps the paths to the grid
        private void SnapPath(Vector3Int start, Vector3Int end, PathManager path)
        {
            start.z = 0;
            end.z = 0;
            Spline spline = path.pathController.spline;

            // Generate path first
            var tilePath = FindPath(start, end);
            int startIndex = 0;
            int endIndex = tilePath.Count - 1;

            for (int i = 0; i < tilePath.Count(); i++) 
            {
                
                var currentTile = tilePath[i];

                if (allTiles.ContainsKey(currentTile)) 
                {
                    // Look for last good starting point in path (Leaving First Path)
                    var tile = allTiles[currentTile];
                    // If start node is a connection
                    if (tile.Connections.FirstOrDefault(node => node.TargetNode == start) != null) 
                    {
                        // Then mark this as the most recent start tiles
                        startIndex = i;
                    }

                    // Look for first ending point in path (Crossing another path)
                    // If end node is a connection
                    if (tile.Connections.FirstOrDefault(node => node.TargetNode == end) != null)
                    {
                        // Then mark this as the end node;
                        endIndex = i;

                        // break loop
                        break;
                    }
                }

                if (currentTile == end) 
                {
                    // Then mark this as the end node;
                    endIndex = i;
                    // break loop
                    break;
                }
            }

            if (spline.GetPointCount() > (endIndex + 1) - startIndex)
            {
                Debug.Log($"[MapSpriteManager] {spline.GetPointCount()} > {(endIndex + 1) - startIndex}");
            }

            // Set path between starting and ending points
            int splineIndex = 0;
            for (int i = startIndex; i < endIndex + 1; i++)
            {
                var tile = tilePath[i];

                // Set grass node
                SetNodeGrass(tile);

                bool lastNode = i == endIndex;
                // if path is on that tile
                if (tileSplineRef.ContainsKey(tile))
                {
                    // follow preset path
                    tileSplineRef[tile].AddChildSpline(new SplineData(spline, splineIndex, path.transform));
                }
                

                Vector3 localTileCenter = path.transform.InverseTransformPoint(MapGrid.CellToWorld(tile));
                bool lastSplineKnot = splineIndex == spline.GetPointCount() - 1;
                bool addNode = lastSplineKnot && !lastNode;
                // Register node
                if (!tileSplineRef.ContainsKey(tile))
                {
                    tileSplineRef.Add(tile,
                        new TileSplineRef(tile, new SplineData(spline, splineIndex, path.transform)));
                }
                else if (!tileSplineRef[tile].ContainsChild(new SplineData(spline, splineIndex, path.transform)))
                {
                    tileSplineRef[tile].AddChildSpline(new SplineData(spline, splineIndex, path.transform));
                }

                // Add or set knot as needed
                if (addNode)
                {
                    spline.InsertPointAt(splineIndex, localTileCenter);
                }
                else
                {
                    spline.SetPosition(splineIndex, localTileCenter);
                }
                // increment spline index
                splineIndex++;

                // Add path to blocked tiles
                blockedTiles.Add(tile);

                // Add tile map
                var tileMap = new MapTilePath(tile);
                if (!allTiles.ContainsKey(tile))
                {
                    allTiles.Add(tile, tileMap);
                }
                else
                {
                    tileMap = allTiles[tile];
                }
                if (i > 0)
                {
                    tileMap.CreateConnection(tilePath[i - 1], start);
                }
                if (i < tilePath.Count() - 1)
                {
                    tileMap.CreateConnection(tilePath[i + 1], end);
                }
            }

            // Set dashed lines to follow full path created
            splineIndex = 0;
            spline = path.lineController.spline;
            Vector3Int current = start;
            bool lastnode = false;
            while (!lastnode)
            {
                lastnode = current == end;
                if (tileSplineRef.ContainsKey(current))
                {
                    tileSplineRef[current].AddChildSpline(new SplineData(spline, splineIndex, path.transform));
                }
                else
                {
                    tileSplineRef.Add(current,
                        new TileSplineRef(current, new SplineData(spline, splineIndex, path.transform)));
                }

                // Set spline to path
                Vector3 localTileCenter = path.transform.InverseTransformPoint(MapGrid.CellToWorld(current));
                bool lastSplineKnot = splineIndex == spline.GetPointCount() - 1;
                bool addNode = lastSplineKnot && !lastnode;

                if (addNode)
                {
                    spline.InsertPointAt(splineIndex, localTileCenter);
                }
                else
                {
                    spline.SetPosition(splineIndex, localTileCenter);
                }

                splineIndex++;

                // Find next tile
                Vector3Int nextNode = default(Vector3Int);
                if (allTiles.ContainsKey(current))
                {
                    foreach (var connection in allTiles[current].Connections)
                    {
                        if (connection.TargetNode == end)
                        {
                            nextNode = connection.NextNode;
                            break;
                        }
                    }
                }

                if (nextNode == current)
                {
                    lastnode = true;
                }
                else
                {
                    current = nextNode;
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
                Debug.Log("[MapSpriteManager] nodePos: " + nodePos + " nodeCelPos: " + MapGrid.CellToWorld(nodeCelPos));
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

        private bool isException(Vector3Int tile, Vector3Int start, Vector3Int end)
        {
            if (allTiles.ContainsKey(tile))
            {
                var con = allTiles[tile].Connections.FirstOrDefault(x => x.TargetNode == end || x.TargetNode == start);
                if (con != null)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
        {
            start.z = 0;
            end.z = 0;
            List<AStarTile> knownTiles = new List<AStarTile>();
            List<Vector3Int> knownTilesRaw = new List<Vector3Int>();
            bool found = false;
            knownTiles.Add(new AStarTile(start, null, start, end));
            knownTilesRaw.Add(start);

            AStarTile currentTile = null;
            // Run AStar
            while (!found)
            {
                var orderedList = knownTiles.OrderBy(t => t.cost);
                if (orderedList.Count() == 0)
                {
                    return new List<Vector3Int>();
                }

                currentTile = orderedList.First();

                if (currentTile.Position == end)
                {
                    found = true;
                }
                else
                {
                    List<Vector3Int> ignore = new List<Vector3Int>();
                    List<Vector3Int> exception = new List<Vector3Int>();

                    foreach (var neigh in currentTile.Neighbors)
                    {
                        if (isException(neigh, start, end))
                        {
                            exception.Add(neigh);
                        }
                    }

                    ignore.AddRange(blockedTiles);
                    ignore.AddRange(knownTilesRaw);
                    var addedTiles = currentTile.CalculateNeighbors(ignore, exception);
                    foreach (var tile in addedTiles)
                    {
                        knownTiles.Add(tile);
                        knownTilesRaw.Add(tile.Position);
                    }

                    knownTiles.RemoveAt(0);
                }
            }

            var lastTile = currentTile;

            // Construct Path From Last Tile
            List<Vector3Int> path = new List<Vector3Int>();
            while (lastTile != null)
            {
                path.Add(lastTile.Position);
                lastTile = lastTile.Previous;
            }

            path.Reverse();

            return path;
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
            Debug.Log("[MapSpriteManager] Map Bounds Recalculated.");
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

        private Vector3Int GetVectorWithZ(int x, int y)
        {
            return new Vector3Int(x, y, (int)GameSettings.MAP_SPRITE_ELEMENTS_Z - 7 + y);
        }

        private Vector3Int GetVectorWithZ(Vector3Int vector)
        {
            return new Vector3Int(vector.x, vector.y, (int)GameSettings.MAP_SPRITE_ELEMENTS_Z - 7 + vector.y);
        }
    }
}