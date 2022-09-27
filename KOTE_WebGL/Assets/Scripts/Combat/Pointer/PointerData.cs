using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PointerData 
{
    public Vector3 Origin;
    public PointerOrigin Type;
    public TargetProfile Targets;

    public PointerData(Vector3 origin, PointerOrigin type, TargetProfile targets) 
    {
        Origin = origin;
        Type = type;
        Targets = targets;
    }
}
