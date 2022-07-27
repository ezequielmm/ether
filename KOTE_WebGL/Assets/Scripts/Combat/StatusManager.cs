using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;

    EnemyManager enemyManager;
    PlayerManager playerManager;

    string entityID => enemyManager?.EnemyData.id ?? "player"; // playerManager?.PlayerData.id;

    void Start()
    {
        if (iconPrefab == null)
        {
            Debug.LogError($"[StatusManager] Missing Icon Prefab.");
        }
        if (iconContainer == null)
        {
            Debug.LogError($"[StatusManager] Missing Icon Container.");
        }
        enemyManager = GetComponentInParent<EnemyManager>();
        playerManager = GetComponentInParent<PlayerManager>();
        if (enemyManager == null && playerManager == null)
        {
            Debug.LogError($"[StatusManager] Manager does not belong either an enemy or a player.");
        }
    }

    private void OnDrawGizmos()
    {
        var rt = iconPrefab.GetComponent<RectTransform>();
        Vector3 size = (Vector3)(rt.rect.max - rt.rect.min);
        Vector3 size2 = size;
        size2.x *= 5;

        Gizmos.color = Color.cyan;
        Utils.GizmoDrawBox(new Bounds(rt.position + transform.position, size), new Vector3(0, 0, transform.position.z));
        Gizmos.color = Color.red;
        Utils.GizmoDrawBox(new Bounds(rt.position + transform.position, size2 * 1.05f), new Vector3(size.x * 2, 0, transform.position.z - 0.1f));
    }

    private void AddStatus() 
    {
        
    }

    private void OnUpdateStatus() 
    {
        
    }

    private STATUS ToEnum(string str) 
    {
        System.Enum.TryParse(str, out STATUS status);
        return status;
    }
}
