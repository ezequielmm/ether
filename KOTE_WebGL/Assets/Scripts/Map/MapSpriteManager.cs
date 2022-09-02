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

        [Tooltip("The range of strength of the spine's knots")]
        [Min(0f)]
        public Vector2 pathCurveStrengthRange;
        [Tooltip("The range of random angle fuzziness on each knot")]
        public Vector2 angleFuzziness;
        [Tooltip("The random position to add to a path")]
        public float pathPositionFuzziness;
        [Tooltip("The random distance between different column.")]
        public Vector2 columnFuzziness;

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


        private class SplinePoint 
        {
            public Vector3Int tileLoc;
            public Spline path;
            public Spline dash;
            public int index;
            public Transform pathTransform;
            public SplinePoint(Vector3Int TileAddress, Spline PathSpline, Spline DashSpline, int Index, Transform transform)
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

            MapSeeds = new List<int>() {
                10,20,30,40,50,60
            };
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

            Random.InitState(MapSeeds[4]);
            InstantiateMapNodes(mapStructure);

            CreateNodeConnections();

            //at this point the map is completed. 
            //we get the maps bounds to help later with scroll limits and animations
            CalculateLocalBounds();

            Debug.Log("last node position: " + nodes[nodes.Count - 1].transform.position + " last node localPosition" +
                      nodes[nodes.Count - 1].transform.localPosition);

            GenerateMapGrid();
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

                        float columnRan = Random.Range(columnFuzziness.x, columnFuzziness.y);

                        // Set Cell's Position
                        newNode.transform.localPosition = new Vector3(columnOffsetCounter + columnRan, yy, GameSettings.MAP_SPRITE_ELEMENTS_Z);

                        // Snap cell to grid
                        newNode.transform.position = MapGrid.layoutGrid.CellToWorld(MapGrid.layoutGrid.WorldToCell(newNode.transform.position));


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
            Random.InitState(MapSeeds[0]);
            GeneratePathBackground();

            Random.InitState(MapSeeds[1]);
            //CurvePath();

            Random.InitState(MapSeeds[2]);
            GenerateMapBackground();
        }

        private class SpineRandom 
        {
            public Vector3Int tilePosition;
            public Vector2 localPosition;
            public Vector3 leftTangent;
            public Vector3 rightTangent;
            public Vector2 tangentStrength;
            public SpineRandom(Vector3Int tilePos, Vector2 locPos, Vector3 leftTan, Vector3 rightTan, Vector2 strength) 
            {
                tilePosition = tilePos;
                localPosition = locPos;
                leftTangent = leftTan;
                rightTangent = rightTan;
                tangentStrength = strength;
            }
        }

        private void CurvePath() 
        {
            Dictionary<Vector3Int, SpineRandom> previousSpots = new Dictionary<Vector3Int, SpineRandom>();
            int count = 0;
            foreach (PathManager path in pathManagers) 
            {
                SpriteShapeController pathSpriteController = path.pathController;
                Spline pathSpline = pathSpriteController.spline;
                Spline dashedSpline = path.lineController.spline;

                Transform pathTransform = pathSpriteController.transform;

                for (int i = 0; i < pathSpline.GetPointCount(); i++) 
                {
                    previousSpots = RunRandomCurve(pathSpline, pathTransform, i, previousSpots);
                    count++;
                }

                for (int i = 0; i < dashedSpline.GetPointCount(); i++)
                {
                    previousSpots = RunRandomCurve(dashedSpline, pathTransform, i, previousSpots);
                    count++;
                }
            }
            Debug.Log($"[Map Sprite Manager] Total nodes motified: {count}");
        }

        private Dictionary<Vector3Int, SpineRandom> RunRandomCurve(Spline spline, Transform pathTransform, int i, Dictionary<Vector3Int, SpineRandom> previousSpots)
        {
            if (i == 0 || i == spline.GetPointCount() - 1) 
            {
                bool first = i == 0;
                Vector3Int tileLoc = MapGrid.layoutGrid.WorldToCell(pathTransform.TransformPoint(spline.GetPosition(i)));

                Vector3 currentPoint = pathTransform.TransformPoint(spline.GetPosition(i));
                Vector3 leftTangent = Vector3.zero;
                Vector3 rightTangent = Vector3.zero;
                float posX = 0;
                float posY = 0;

                if (previousSpots.TryGetValue(tileLoc, out SpineRandom referenceSpine))
                {
                    posX = referenceSpine.localPosition.x;
                    posY = referenceSpine.localPosition.y;
                }

                if (!first)
                {
                    float angleRangeL = Random.Range(angleFuzziness.x, angleFuzziness.y);
                    Vector3 leftPoint = pathTransform.TransformPoint(spline.GetPosition(i - 1));
                    float spineStrengthL = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                    leftTangent = Quaternion.Euler(0, 0, angleRangeL) * (leftPoint - currentPoint).normalized * spineStrengthL;
                }
                if (first)
                {
                    float angleRangeR = Random.Range(angleFuzziness.x, angleFuzziness.y);
                    Vector3 rightPoint = pathTransform.TransformPoint(spline.GetPosition(i + 1));
                    float spineStrengthR = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                    rightTangent = Quaternion.Euler(0, 0, angleRangeR) * (rightPoint - currentPoint).normalized * spineStrengthR;
                }

                
                spline.SetPosition(i, spline.GetPosition(i) + new Vector3(posX, posY, 0));
                if (!first)
                    spline.SetLeftTangent(i, leftTangent);
                if (first)
                    spline.SetRightTangent(i, rightTangent);
            }
            else if (i <= 0 || i >= spline.GetPointCount() - 1) // out of range
            {
                return previousSpots;
            }
            else
            {
                Vector3Int tileLoc = MapGrid.layoutGrid.WorldToCell(pathTransform.TransformPoint(spline.GetPosition(i)));

                Vector3 leftPoint = pathTransform.TransformPoint(spline.GetPosition(i - 1));
                Vector3 currentPoint = pathTransform.TransformPoint(spline.GetPosition(i));
                Vector3 rightPoint = pathTransform.TransformPoint(spline.GetPosition(i + 1));

                Vector3 leftTangent = (leftPoint - currentPoint).normalized;
                Vector3 rightTangent = (rightPoint - currentPoint).normalized;

                Vector3 averageDir = (leftTangent + rightTangent).normalized;

                float spineStrengthL = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);
                float spineStrengthR = Random.Range(pathCurveStrengthRange.x, pathCurveStrengthRange.y);

                float angleRangeL = Random.Range(angleFuzziness.x, angleFuzziness.y);
                float angleRangeR = Random.Range(angleFuzziness.x, angleFuzziness.y);

                float posX = Random.Range(-pathPositionFuzziness, pathPositionFuzziness);
                float posY = Random.Range(-pathPositionFuzziness, pathPositionFuzziness);

                if (previousSpots.TryGetValue(tileLoc, out SpineRandom referenceSpine))
                {
                    leftTangent = referenceSpine.leftTangent;
                    rightTangent = referenceSpine.rightTangent;
                    spineStrengthL = referenceSpine.tangentStrength.x;
                    spineStrengthR = referenceSpine.tangentStrength.y;
                    posX = referenceSpine.localPosition.x;
                    posY = referenceSpine.localPosition.y;
                }
                else
                {
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
                    var spineRan = new SpineRandom(tileLoc, new Vector2(posX, posY), leftTangent, rightTangent, new Vector2(spineStrengthL, spineStrengthR));
                    previousSpots.Add(tileLoc, spineRan);
                }

                spline.SetPosition(i, spline.GetPosition(i) + new Vector3(posX, posY, 0));
                spline.SetLeftTangent(i, leftTangent * spineStrengthL);
                spline.SetRightTangent(i, rightTangent * spineStrengthR);
            }
            return previousSpots;
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
        {//Generate the grass around the path and set up the path in the process
            // Clear out path dictionary for generation
            if (mapPaths != null) { mapPaths.Clear(); }
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
                Vector3Int end = MapGrid.WorldToCell(path.transform.TransformPoint(spline.GetPosition(spline.GetPointCount() - 1)));
                SnapPath(start, end, path);
                break;
            }
        }

        private void SetNodeGrass(Vector3Int node) 
        {
            // we have to set the z to a constant, as for some reason you can two tiles in the same spot with different z levels
            node.z = (int)GameSettings.MAP_SPRITE_ELEMENTS_Z;

            int randomTile = Random.Range(0, grassTiles.Length);
            MapGrid.SetTile(node, grassTiles[randomTile]);
        }

        private void SnapPath(Vector3Int start, Vector3Int end, PathManager path) 
        {
            Spline spline = path.pathController.spline;
            StartCoroutine(FindPath(start, end));
            //var tilePath = FindPath(start, end);
            //for (int i = 0; i < tilePath.Count(); i++) 
            //{
            //    var tile = tilePath[i];

            //    // Set tile grass
            //    SetNodeGrass(tile);

            //    // Set Spline to Path
            //    Vector3 localTileCenter = path.transform.InverseTransformPoint(MapGrid.CellToWorld(tile));

            //    bool addNode = i != tilePath.Count() - 1 && i == spline.GetPointCount() - 1;
            //    if (addNode)
            //    {
            //        spline.InsertPointAt(i, localTileCenter);
            //    } 
            //    else 
            //    {
            //        spline.SetPosition(i, localTileCenter);
            //    }

            //    // Add path to blocked tiles (except first and last)
            //    if (i != 0 && i != tilePath.Count() - 1) 
            //    {
            //        blockedTiles.Add(tile);
            //    }
            //}
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

        List<Vector3Int> blockedTiles = new List<Vector3Int>();
        private IEnumerator /*List<Vector3Int>*/ FindPath(Vector3Int start, Vector3Int end) 
        {
            List<AStarTile> knownTiles = new List<AStarTile>();
            List<Vector3Int> knownTilesRaw = new List<Vector3Int>();
            bool found = false;
            knownTiles.Add(new AStarTile(start, null, start, end, MapGrid));
            knownTilesRaw.Add(start);

            // Run AStar
            while (!found) 
            {
                knownTiles.OrderBy(t => t.cost);
                SetNodeGrass(knownTiles.First().Position);
                if (knownTiles.First().Position == end)
                {
                    found = true;
                }
                else 
                {
                    var addedTiles = knownTiles.First().CalculateNeighbors(blockedTiles, knownTilesRaw);
                    foreach (var tile in addedTiles) 
                    {
                        knownTiles.Add(tile);
                        knownTilesRaw.Add(tile.Position);
                    }
                    knownTiles.RemoveAt(0);
                }
                //if (knownTiles.Count > 10000)
                //    return new List<Vector3Int>();
                yield return new WaitForSeconds(1);
            }
            var lastTile = knownTiles.First();
            
            // Construct Path From Last Tile
            List<Vector3Int> path = new List<Vector3Int>();
            while (lastTile != null) 
            {
                path.Add(lastTile.Position);
                lastTile = lastTile.Previous;
            }

            path.Reverse();

            //return path;
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