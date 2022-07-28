using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;

    [SerializeField]
    EnemyManager enemyManager;
    [SerializeField]
    PlayerManager playerManager;

    List<StatusData.Status> statusList;

    string entityType => (enemyManager == null ? "player" : "enemy");
    int entityID => enemyManager?.EnemyData.enemyId ?? playerManager?.PlayerData.playerId ?? -1;

    static bool askedForStatus = false;
    bool statusSet = false;

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
        if(enemyManager == null)
            enemyManager = GetComponentInParent<EnemyManager>();
        if(playerManager == null)
            playerManager = GetComponentInParent<PlayerManager>();
        if (enemyManager == null && playerManager == null)
        {
            Debug.LogError($"[StatusManager] Manager does not belong either an enemy or a player.");
        }
        GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.AddListener(OnUpdateStatus);
        iconContainer.SetFadeSpeed(GameSettings.STATUS_FADE_SPEED);
        iconContainer.fadeOnCreate = true;
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

    private void onTurnChange() 
    {
        if (!askedForStatus)
        {
            askForStatus();
        }
    }

    private void DrawStatus() 
    {
        iconContainer.ClearIcons();
        foreach (var status in statusList) 
        {
            GameObject iconObject = Instantiate(iconPrefab);
            var statusIcon = iconPrefab.GetComponent<StatusIcon>();
            statusIcon.Initialize();

            STATUS stat = ToEnum(status.name);
            statusIcon.SetValue(status.counter);
            statusIcon.SetIcon(stat);
            statusIcon.SetTooltip(status.description);

            iconContainer.AddIcon(iconObject);
        }
        iconContainer.ReorganizeSprites();

        askedForStatus = false;
        statusSet = true;
    }

    private void Update()
    {
        if (!statusSet && !askedForStatus)
        {
            askForStatus();
        }
    }

    private void askForStatus()
    {
        askedForStatus = true;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Statuses);
    }

    private void OnUpdateStatus(StatusData status) 
    {
        if (status.targetEntity != entityType || status.id != entityID) return;

        statusList = status.statuses;
        DrawStatus();
    }

    private STATUS ToEnum(string str) 
    {
        System.Enum.TryParse(str, out STATUS status);
        return status;
    }
}
