using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using map;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class MapSpriteManagerEditModeTests
{
    private MapSpriteManager _mapSpriteManager;

    [SetUp]
    public void Setup()
    {
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/MapElements.prefab");
        _mapSpriteManager = mapPrefab.GetComponent<MapSpriteManager>();
    }

    [Test]
    public void DoesMapContainterExist()
    {
        Assert.IsNotNull(_mapSpriteManager.mapContainer);
    }

    [Test]
    public void DoesNodePrefabExist()
    {
        Assert.IsNotNull(_mapSpriteManager.nodePrefab);
    }

    [Test]
    public void DoesPlayerIconExist()
    {
        Assert.IsNotNull(_mapSpriteManager.playerIcon);
    }

    [Test]
    public void DoesPortalAnimationExist()
    {
        Assert.IsNotNull(_mapSpriteManager.portalAnimation);
    }

    [Test]
    public void DoesNodesHolderExist()
    {
        Assert.IsNotNull(_mapSpriteManager.nodesHolder);
    }

    [Test]
    public void DoesMapGridExist()
    {
        Assert.IsNotNull(_mapSpriteManager.MapGrid);
    }

    [Test]
    public void DoGrassTilesExist()
    {
        Assert.IsNotNull(_mapSpriteManager.actTileLists[0].grassTiles);
        Assert.AreNotEqual(0, _mapSpriteManager.actTileLists[0].grassTiles.Length);
    }

    [Test]
    public void DoMountianTilesExist()
    {
        Assert.IsNotNull(_mapSpriteManager.actTileLists[0].mountainTiles);
        Assert.AreNotEqual(0, _mapSpriteManager.actTileLists[0].mountainTiles.Length);
    }

    [Test]
    public void DoForestTilesExist()
    {
        Assert.IsNotNull(_mapSpriteManager.actTileLists[0].forestTiles);
        Assert.AreNotEqual(0, _mapSpriteManager.actTileLists[0].forestTiles.Length);
    }

    [Test]
    public void IsPathCurveStrengthRangeMinMaxed()
    {
        Assert.GreaterOrEqual(_mapSpriteManager.pathCurveStrengthRange.y, _mapSpriteManager.pathCurveStrengthRange.x);
    }

    [Test]
    public void IsAngleFuzzinessMinMaxed()
    {
        Assert.GreaterOrEqual(_mapSpriteManager.angleFuzziness.y, _mapSpriteManager.angleFuzziness.x);
    }

    [Test]
    public void IsAngleFuzzinessClampedInRange()
    {
        Assert.LessOrEqual(_mapSpriteManager.angleFuzziness.x, 360);
        Assert.GreaterOrEqual(_mapSpriteManager.angleFuzziness.x, -360);
        Assert.LessOrEqual(_mapSpriteManager.angleFuzziness.y, 360);
        Assert.GreaterOrEqual(_mapSpriteManager.angleFuzziness.y, -360);
    }

    [Test]
    public void IsColumnFuzzinessMinMaxed()
    {
        Assert.GreaterOrEqual(_mapSpriteManager.columnFuzziness.y, _mapSpriteManager.columnFuzziness.x);
    }

    [Test]
    public void IsColumnFuzzinessClampedInRange()
    {
        Assert.LessOrEqual(_mapSpriteManager.columnFuzziness.x, GameSettings.MAP_SPRITE_NODE_X_OFFSET);
        Assert.GreaterOrEqual(_mapSpriteManager.columnFuzziness.x, 0);
        Assert.LessOrEqual(_mapSpriteManager.columnFuzziness.y, GameSettings.MAP_SPRITE_NODE_X_OFFSET);
        Assert.GreaterOrEqual(_mapSpriteManager.columnFuzziness.y, 0);
    }
}