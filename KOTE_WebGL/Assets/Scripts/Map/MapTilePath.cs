using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class MapTilePath : HexTile
{
    public List<Vector3Int> Neighbors => neighbors(Position);
    public class Connection 
    {
        public Vector3Int NextNode { get; private set; }
        public Vector3Int TargetNode { get; private set; }
        public Connection(Vector3Int next, Vector3Int target) 
        {
            NextNode = next;
            TargetNode = target;
        }
    }

    /// <summary>
    /// List of all connections from this node.
    /// </summary>
    public List<Connection> Connections;
    /// <summary>
    /// The position of this node.
    /// </summary>
    public Vector3Int Position;

    public MapTilePath(Vector3Int currentPosition) 
    {
        Connections = new List<Connection>();
        Position = currentPosition;
    }
}

public class AStarTile : HexTile
{
    public List<Vector3Int> Neighbors => neighbors(Position);
    public Vector3Int Position;
    public AStarTile Previous;
    public float g { get; private set; }
    public float h { get; private set; }
    public float cost => g + h;

    private Vector3Int _start, _end;
    Tilemap _mapGrid;
    public AStarTile(Vector3Int position, AStarTile previous, Vector3Int start, Vector3Int end, Tilemap mapGrid) 
    {
        Position = position;
        g = Vector3.Distance(mapGrid.CellToWorld(position), mapGrid.CellToWorld(start));
        h = Vector3.Distance(mapGrid.CellToWorld(position), mapGrid.CellToWorld(end));
        Previous = previous;
        _start = start;
        _end = end;
        _mapGrid = mapGrid;
    }
    public List<AStarTile> CalculateNeighbors(List<Vector3Int> ignoredTiles, List<Vector3Int> knownTiles) 
    {
        List<AStarTile> result = new List<AStarTile>();
        foreach (Vector3Int neighbor in Neighbors) 
        {
            if (ignoredTiles.Contains(neighbor))
                continue;
            if(knownTiles.Contains(neighbor))
                continue;
            if (Previous?.Position == neighbor)
                continue;
            result.Add(new AStarTile(neighbor, this, _start, _end, _mapGrid));
        }
        return result;
    }
}

public class HexTile 
{
    static Vector3Int
    LEFT = new Vector3Int(-1, 0, 0),
    RIGHT = new Vector3Int(1, 0, 0),
    DOWN = new Vector3Int(0, -1, 0),
    DOWNLEFT = new Vector3Int(-1, -1, 0),
    DOWNRIGHT = new Vector3Int(1, -1, 0),
    UP = new Vector3Int(0, 1, 0),
    UPLEFT = new Vector3Int(-1, 1, 0),
    UPRIGHT = new Vector3Int(1, 1, 0);

    static Vector3Int[] directions_when_y_is_even =
          { LEFT, RIGHT, DOWN, DOWNLEFT, UP, UPLEFT };
    static Vector3Int[] directions_when_y_is_odd =
          { LEFT, RIGHT, DOWN, DOWNRIGHT, UP, UPRIGHT };

    protected List<Vector3Int> neighbors(Vector3Int node)
    {
        Vector3Int[] directions = (node.y % 2) == 0 ?
             directions_when_y_is_even :
             directions_when_y_is_odd;
        List<Vector3Int> result = new List<Vector3Int>();
        foreach (var direction in directions)
        {
            Vector3Int neighborPos = node + direction;
            result.Add(neighborPos);
        }
        return result;
    }
}

public class SplineData
{
    public Vector3Int tileLoc;
    public Spline path;
    public int index;
    public Transform pathTransform;
    public SplineData(Vector3Int TileAddress, Spline PathSpline, int Index, Transform transform)
    {
        tileLoc = TileAddress;
        path = PathSpline;
        index = Index;
        pathTransform = transform;
    }
}
