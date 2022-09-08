using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
using UnityEngine.Tilemaps;

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

    public void CreateConnection(Vector3Int otherTile, Vector3Int targetTile) 
    {
        Connections.Add(new Connection(otherTile, targetTile));
    }

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
    public AStarTile(Vector3Int position, AStarTile previous, Vector3Int start, Vector3Int end) 
    {
        Position = position;
        if (previous != null)
        {
            g = Vector3.Distance(position,previous.Position) + previous.g;
        }
        h = Vector3.Distance(position, end);
        Previous = previous;
        _start = start;
        _end = end;
    }
    public List<AStarTile> CalculateNeighbors(List<Vector3Int> ignoredTiles, List<Vector3Int> allowedTiles) 
    {
        List<AStarTile> result = new List<AStarTile>();
        foreach (Vector3Int neighbor in Neighbors) 
        {
            bool isIgnored = ignoredTiles.Contains(neighbor) && !allowedTiles.Contains(neighbor);
            bool lastNode = Previous?.Position == neighbor;
            if ((isIgnored || lastNode) && (_end != neighbor))
                continue;
            result.Add(new AStarTile(neighbor, this, _start, _end));
        }
        return result.OrderBy(a => Random.Range(0f, 1f)).ToList();
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

public class TileSplineRef 
{
    public Vector3Int Position;
    public SplineData MasterSpline;
    public List<SplineData> ChildrenSplines;
    public bool MasterRandomized = false;

    public TileSplineRef(Vector3Int position, SplineData mainData) 
    {
        Position = position;
        MasterSpline = mainData;
        ChildrenSplines = new List<SplineData>();
    }

    public bool ContainsChild(SplineData other) 
    {
        foreach (var child in ChildrenSplines) 
        {
            if(other.Transform.Equals(child.Transform))
                return true;
        }
        return false;
    }

    public void AddChildSpline(SplineData childData) 
    {
        if (childData == MasterSpline || ChildrenSplines.Contains(childData))
        {
            return;
        }
        if (MasterSpline.IsEndPiece && !childData.IsEndPiece)
        {
            ChildrenSplines.Add(MasterSpline);
            MasterSpline = childData;
        }
        else
        {
            ChildrenSplines.Add(childData);
        }
    }

    public void EnforceSplineMatch() 
    {
        foreach (var child in ChildrenSplines) 
        {
            child.Position = MasterSpline.Position;
            if (!child.IsEndPiece) 
            {
                child.Spline.SetTangentMode(child.Index, ShapeTangentMode.Continuous);
                child.LeftTangent = MasterSpline.LeftTangent;
                child.RightTangent = MasterSpline.RightTangent;
            }
        }
    }
}
public class SplineData
{
    public Spline Spline;
    public int Index;
    public Transform Transform;


    public Vector3 LeftTangent {
        get 
        {
            return Spline.GetLeftTangent(Index);
        }
        set 
        {
            Spline.SetLeftTangent(Index, value);
        }
    }
    public Vector3 RightTangent
    {
        get
        {
            return Spline.GetRightTangent(Index);
        }
        set
        {
            Spline.SetRightTangent(Index, value);
        }
    }
    public Vector3 Position {
        get {
            return Transform.TransformPoint(Spline.GetPosition(Index));
        }
        set 
        {
            try
            {
                Spline.SetPosition(Index, Transform.InverseTransformPoint(value));
            }
            catch 
            {
                Debug.LogError($"[SplineData] Could not set point at index {Index} | {value}");
            }
        }
    }
    public bool IsEndPiece { get {
            return Index == 0 || Index == Spline.GetPointCount() - 1;
        }
    }

    public SplineData(Spline spline, int index, Transform tranform)
    {
        Spline = spline;
        Index = index;
        Transform = tranform;
    }

    public Vector3Int GetTilePosition(Tilemap map) 
    {
        return map.WorldToCell(Position);
    }
}
