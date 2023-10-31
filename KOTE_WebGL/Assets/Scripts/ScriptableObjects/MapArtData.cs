using System.Linq;
using UnityEngine;

namespace DefaultNamespace.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapTileData", menuName = "Map/MapTileData")]
    public class MapArtData : ScriptableObject
    {
        [System.Serializable]
        public struct MapTile
        {
            public float percentageStep;
            public MapTileList tileList;
            public GameObject combatBackground;
        }
        
        public MapTile[] mapTiles;
        
        public GameObject bossBackground;

        public MapTileList GetTilesFromStep(int currentStep, int maxStep)
        {
            var percentage = (float)currentStep / (float)maxStep;
            foreach (var mapTile in mapTiles)
            {
                if (percentage <= mapTile.percentageStep)
                {
                    return mapTile.tileList;
                }
            }

            return mapTiles.First().tileList;
        }
    }
}