using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static StatusData;

public class StatusManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;

    [SerializeField]
    EnemyManager enemyManager;
    [SerializeField]
    PlayerManager playerManager;

    [SerializeField]
    BoxCollider2D statusCollider;

    List<Status> statusList = new List<Status>();
    Dictionary<string, GameObject> statusIconList = new Dictionary<string, GameObject>();

    string entityType => (enemyManager == null ? "player" : "enemy");
    string entityID => enemyManager?.EnemyData?.id ?? playerManager?.PlayerData?.id ?? "-1";

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
        GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.AddListener(OnSetStatus);
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(OnTurnChange);

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(DeactivateCollider);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(ActivateCollider);

        iconContainer.SetFadeSpeed(GameSettings.STATUS_FADE_SPEED);
        iconContainer.fadeOnCreate = true;
    }

    private void ActivateCollider(string _)
    {
        if (statusCollider != null)
            statusCollider.enabled = true;
    }
    private void DeactivateCollider(PointerData _)
    {
        if (statusCollider != null)
            statusCollider.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        List<Tooltip> list = new List<Tooltip>();
        foreach (StatusIcon icon in GetComponentsInChildren<StatusIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        gameObject.GetComponentInParent<ITooltipSetter>().SetTooltip(list);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }

    private void OnDrawGizmos()
    {
        var rt = iconPrefab.GetComponent<RectTransform>();
        Vector3 size = (Vector3)(rt.rect.max - rt.rect.min);
        Vector3 size2 = size;
        size2.x *= 5;

        Gizmos.color = Color.cyan;
        GizmoExtensions.DrawBox(new Bounds(rt.position + transform.position, size), new Vector3(0, 0, transform.position.z));
        Gizmos.color = Color.red;
        GizmoExtensions.DrawBox(new Bounds(rt.position + transform.position, size2 * 1.05f), new Vector3(size.x * 2, 0, transform.position.z - 0.1f));
    }

    private GameObject CreateIcon(Status status) 
    {
        GameObject iconObject = Instantiate(iconPrefab);
        var statusIcon = iconObject.GetComponentInChildren<StatusIcon>();
        statusIcon.Initialize();
        SetStatusInfo(status, statusIcon);
        return iconObject;
    }

    private void SetStatusInfo(Status status, StatusIcon statusIcon)
    {
        STATUS stat = ToEnum(status.name);
        statusIcon.SetValue(status.counter);
        statusIcon.SetIcon(stat);
        statusIcon.SetTooltip(status.name, status.description);
    }

    private void SetStatusInfo(Status status, GameObject iconObject) 
    {
        var statusIcon = iconObject.GetComponentInChildren<StatusIcon>();
        SetStatusInfo(status, statusIcon);
    }

    private void DrawStatus(List<Status> newStatus) 
    {
        foreach (var status in newStatus) 
        {
            var hasKey = statusIconList.ContainsKey(status.name);
            if (!hasKey) // New not in known
            {
                var newIcon = CreateIcon(status);
                statusIconList.Add(status.name, newIcon);
                iconContainer.AddIcon(newIcon);
            }
            else // New and Known Match
            {
                var icon = statusIconList[status.name];
                SetStatusInfo(status, icon);
            }
        }
        List<string> keysToDelete = new List<string>();
        foreach (string key in statusIconList.Keys) 
        {
            if (newStatus.Find(status => status.name == key) == null) // Known not in New
            {
                iconContainer.DeleteIcon(statusIconList[key]);
                keysToDelete.Add(key);
            }
        }
        foreach (string key in keysToDelete) { statusIconList.Remove(key); }
        iconContainer.ReorganizeSprites();

        statusCollider.enabled = true;
        statusCollider.offset = iconContainer.Bounds.center + iconContainer.transform.localPosition;
        statusCollider.size = iconContainer.Bounds.size;
    }

    private void OnTurnChange(string who) 
    {
        askedForStatus = false;
        statusSet = false;
    }

    private void Update()
    {
        if (!statusSet && !askedForStatus)
        {
            statusSet = true;
            AskForStatus();
        }
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            bool hovered = false;
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.BoxCastAll(worldPoint, Vector2.one * 0.01f, 0, Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == statusCollider)
                {
                    hovered = true;
                    break;
                }
            }
            Debug.Log($"[StatusManager] Is Hovering? {hovered} | Total Colliders? {hits.Length}");
        }
    }

    private void AskForStatus()
    {
        Debug.Log("[Status Manager] Status Requested");
        askedForStatus = true;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Statuses);
    }

    public void UpdateStatus(List<StatusData.Status> newStatuses) 
    {
        foreach (var status in newStatuses)
        {
            bool set = false;
            for (int i = 0; i < statusList.Count; i++)
            {
                if (statusList[i].name == status.name)
                {
                    statusList[i] = status;
                    set = true;
                    break;
                }
            }
            if (!set)
            {
                statusList.Add(status);
            }
        }
        DrawStatus(statusList);
    }

    private void OnSetStatus(StatusData status) 
    {
        if ((status.targetEntity != entityType || status.id != entityID) && (status.targetEntity != "all")) return;

        statusList = status.statuses;
        DrawStatus(status.statuses);
    }

    private STATUS ToEnum(string str) 
    {
        System.Enum.TryParse(str, out STATUS status);
        return status;
    }
}
