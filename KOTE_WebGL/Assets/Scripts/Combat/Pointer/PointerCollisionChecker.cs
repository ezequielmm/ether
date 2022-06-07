using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerCollisionChecker : MonoBehaviour
{
    public PointerManager pointerManager;
    // these are triggers to avoid needing rigidbodies
    //TODO get the enemy's id and make sure it's the same one on exit
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            pointerManager.overEnemy = true;
            Debug.Log("over enemy");
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
           pointerManager.overEnemy = false;
        }
    }
}
