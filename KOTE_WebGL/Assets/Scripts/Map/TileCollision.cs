using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollision : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("collision happened");
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint point = collision.GetContact(i);
            Debug.Log(point + "index: " + i + " contact name: " + point.otherCollider.name);
        }
    }
}
