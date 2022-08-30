using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private static Cursor cursor;
    public static Cursor instance 
    {
        get 
        {
            if (cursor == null) 
            {
                var go = new GameObject();
                go.name = "Cursor (Default)";
                cursor = go.AddComponent<Cursor>();
            }
            return cursor;
        }
    }

    private void Awake()
    {
        if (cursor == null && cursor != this)
        {
            cursor = this;
        }
        else
        {
            Debug.LogWarning("[Cursor] You can have only one cursor per scene.");
            Destroy(this);
        }
    }

    void Update()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
