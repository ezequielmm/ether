using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(fileName="MapTileList", menuName = "ScriptableObjects/SpriteLists/MapTileList")]
public class MapTileList : ScriptableObject
{
    public Tile[] grassTiles;
    public Tile[] lakeTiles;
    public Tile[] mountainTiles;
    public Tile[] forestTiles;
}