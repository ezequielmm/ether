using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using map;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MapSpriteManagerTests
{
    private MapSpriteManager _mapSpriteManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        /*
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/MapElements.prefab");
        mapPrefab.SetActive(true);
        _mapSpriteManager = mapPrefab.GetComponent<MapSpriteManager>();
        yield return null;
        */
        PlayerPrefs.DeleteKey("session_token");
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/Expedition");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        _mapSpriteManager = GameObject.Find("MapElements").GetComponent<MapSpriteManager>();
    }

    [Test]
    public void MapContainerActiveOnStart()
    {
        Assert.True(_mapSpriteManager.mapContainer.activeInHierarchy);
    }

    [Test]
    public void PlayerIconIsHiddenOnStart()
    {
        Assert.False(_mapSpriteManager.playerIcon.activeInHierarchy);
        Assert.AreNotEqual(true, _mapSpriteManager.playerIcon.activeSelf);
    }
   
    [Test]
    public void PortalAnimationHasCorrectSortingLayer()
    {
        Assert.AreEqual(GameSettings.MAP_ELEMENTS_SORTING_LAYER_NAME,
            _mapSpriteManager.portalAnimation.GetComponent<Renderer>().sortingLayerName);
    }

    [Test]
    public void MapPanelToggledOnEvent()
    {
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        Assert.False(_mapSpriteManager.mapContainer.activeInHierarchy);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(true);
        Assert.True(_mapSpriteManager.mapContainer.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator CorrectNumberOfNodesGenerated()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(3, 2);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        yield return null;
        int numNodes = _mapSpriteManager.nodesHolder.GetComponentsInChildren<NodeData>().Length;
        Assert.AreEqual(3, numNodes);
    }

    [UnityTest]
    public IEnumerator CorrectNumberOfActiveNodesGenerated()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(3, 2);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        yield return null;
        NodeData[] createdNodes = _mapSpriteManager.nodesHolder.GetComponentsInChildren<NodeData>();
        int numActiveNodes = 0;
        foreach (NodeData node in createdNodes)
        {
            if (node.status == NODE_STATUS.active) numActiveNodes++;
        }

        Assert.AreEqual(1, numActiveNodes);
    }

    [UnityTest]
    public IEnumerator PlayerIconPlacedAtCorrectPoint()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(3, 2);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        yield return null;
        List<NodeData> createdNodes = _mapSpriteManager.nodesHolder.GetComponentsInChildren<NodeData>().ToList();
        NodeData activeNode = createdNodes.Find(node => node.status == NODE_STATUS.active);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(_mapSpriteManager.playerIcon.transform.localPosition.x,
            activeNode.transform.localPosition.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(_mapSpriteManager.playerIcon.transform.localPosition.y,
            activeNode.transform.localPosition.y);
    }

    [Test]
    public void PlayerIconPlacedAtCorrectPointWhenNodeSelected()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(3, 2);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.Invoke(2);
        List<NodeData> createdNodes = _mapSpriteManager.nodesHolder.GetComponentsInChildren<NodeData>().ToList();
        NodeData correctNode = createdNodes.Find(node => node.id == 2);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(_mapSpriteManager.playerIcon.transform.localPosition.x,
            correctNode.transform.localPosition.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(_mapSpriteManager.playerIcon.transform.localPosition.y,
            correctNode.transform.localPosition.y);
    }

    [UnityTest]
    public IEnumerator MapPanelMovesTowardsPlayerOnDoubleClick()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(20, 15);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        yield return null;
        Vector3 mapPosition = _mapSpriteManager.nodesHolder.transform.position;
        Vector3 playerPosition = _mapSpriteManager.playerIcon.transform.position;
        yield return null;
        GameManager.Instance.EVENT_MAP_MASK_DOUBLECLICK.Invoke();
        yield return new WaitForSeconds(1);
        Assert.AreNotEqual(mapPosition, playerPosition);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(mapPosition.x,
            _mapSpriteManager.nodesHolder.transform.position.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(mapPosition.y,
            _mapSpriteManager.nodesHolder.transform.position.y);
        if (playerPosition.x < mapPosition.x)
        {
            Assert.Less(mapPosition.x, _mapSpriteManager.nodesHolder.transform.position.x);
        }
        else
        {
            Assert.Greater(mapPosition.x, _mapSpriteManager.nodesHolder.transform.position.x);
        }
    }

    [Test]
    public void DoesClickingMapIconFireEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggle) => { eventFired = true; });
        _mapSpriteManager.mapContainer.SetActive(true);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        Assert.True(eventFired);
        eventFired = false;
        _mapSpriteManager.mapContainer.SetActive(false);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesClickingMapIconDeactivateNodeInteractions()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(20, 15);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        _mapSpriteManager.mapContainer.SetActive(false);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        yield return null;
        NodeData[] createdNodes = _mapSpriteManager.nodesHolder.GetComponentsInChildren<NodeData>();
        int nodesInteractionsDisabled = 0;
        foreach (NodeData node in createdNodes)
        {
            if (node.nodeClickDisabled) nodesInteractionsDisabled++;
        }
        Assert.AreEqual(20, nodesInteractionsDisabled);
    }

    [Test]
    public void DoesClickingMapIconToggleMapContainer()
    {
        _mapSpriteManager.mapContainer.SetActive(true);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        Assert.False(_mapSpriteManager.mapContainer.activeSelf);

        _mapSpriteManager.mapContainer.SetActive(false);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        Assert.True(_mapSpriteManager.mapContainer.activeSelf);
    }

    [UnityTest]
    public IEnumerator DoesClickingScrollButtonMoveMap()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(20, 15);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        Vector3 mapPosition = _mapSpriteManager.nodesHolder.transform.position;
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, true);
        yield return null;
        Assert.Less(_mapSpriteManager.nodesHolder.transform.position.x, mapPosition.x);
        yield return null;
        mapPosition = _mapSpriteManager.nodesHolder.transform.position;
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, false);
        yield return null;
        Assert.Greater(_mapSpriteManager.nodesHolder.transform.position.x, mapPosition.x);
    }

    [UnityTest]
    public IEnumerator DoesDraggingMapMoveMap()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(20, 15);
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        Vector3 mapPosition = _mapSpriteManager.nodesHolder.transform.position;
        GameManager.Instance.EVENT_MAP_SCROLL_DRAG.Invoke(Vector3.zero);
        yield return null;
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(_mapSpriteManager.nodesHolder.transform.position.x,
            mapPosition.x);
    }

    [UnityTest]
    public IEnumerator DoesPortalAnimationPlayOnEvent()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestPortalMap();
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(testMap);
        yield return null;
        GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.Invoke(testMap);
        yield return null;
        ParticleSystem portalAnimation = _mapSpriteManager.portalAnimation;
        Assert.True(portalAnimation.isPlaying);
    }

    [UnityTest]
    public IEnumerator DoesRevealingMapReturnToPlayer()
    {
        SWSM_MapData testMap = TestUtils.GenerateTestMap(20, 15);
        GameManager.Instance.EVENT_MAP_REVEAL.Invoke(testMap);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(GameSettings.MAP_SCROLL_ANIMATION_DURATION);
        yield return new WaitForSeconds(2);
        float disToKnight = _mapSpriteManager.playerIcon.transform.position.x -
                            _mapSpriteManager.nodesHolder.transform.position.x;
        // Subtract desired knight position by distance to get node position.
        float targetx = ((Camera.main.orthographicSize * Camera.main.aspect)
                         * GameSettings.KNIGHT_SCREEN_POSITION_ON_CENTER) - disToKnight;
        yield return new WaitForSeconds(GameSettings.MAP_SCROLL_ANIMATION_DURATION);
        _mapSpriteManager.playerIcon.transform.TransformPoint(_mapSpriteManager.playerIcon.transform.position);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(
            targetx,
            _mapSpriteManager.nodesHolder.transform.position.x);
    }
}