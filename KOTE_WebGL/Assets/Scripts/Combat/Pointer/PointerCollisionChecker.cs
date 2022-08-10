using UnityEngine;

public class PointerCollisionChecker : MonoBehaviour
{
    public PointerManager pointerManager;
    // these are triggers to avoid needing rigidbodies
    //TODO get the enemy's id and make sure it's the same one on exit
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("[Pointer] Collider Enter");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("[Pointer] Over Enemy");
            pointerManager.overEnemy = true;
            pointerManager.enemyData = collision.gameObject.GetComponentInChildren<EnemyManager>().EnemyData;
            if (pointerManager.enemyData == null) 
            {
                pointerManager.overEnemy = false;
                return;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
           pointerManager.overEnemy = false;
        }
    }
}
