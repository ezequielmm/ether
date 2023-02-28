using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataManager
{
    protected WebRequesterManager webRequest => WebRequesterManager.Instance;
    protected WebSocketManager socketRequest => WebSocketManager.Instance;

    protected DataManager()
    {
    }
}
