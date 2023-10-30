using UnityEngine;

namespace DefaultNamespace.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapTileData", menuName = "Map/MapTileData")]
    public class MapTileData : ScriptableObject
    {
        [System.Serializable]
        public struct MapTile
        {
            public float percentageStep;
            public MapTileList tileList;
        }
        
        public MapTile[] mapTiles;
    }
}