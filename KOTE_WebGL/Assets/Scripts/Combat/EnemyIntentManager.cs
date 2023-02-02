using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyIntentManager : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;

    [SerializeField]
    BoxCollider2D intentCollider;

    EnemyManager enemyManager;
    static bool askedForIntent;
    bool intentSet;

    string enemyId => enemyManager?.EnemyData?.id ?? string.Empty;

    void Start()
    {
        if (iconPrefab == null)
        {
            Debug.LogError($"[EnemyIntentManager] Missing Icon Prefab.");
        }
        if (iconContainer == null)
        {
            Debug.LogError($"[EnemyIntentManager] Missing Icon Container.");
        }
        enemyManager = GetComponentInParent<EnemyManager>();
        if (enemyManager == null) 
        {
            Debug.LogError($"[EnemyIntentManager] Manager does not belong to an enemy.");
        }
        GameManager.Instance.EVENT_UPDATE_INTENT.AddListener(OnUpdateIntent);
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(onTurnChange);

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(DeactivateCollider);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(ActivateCollider);

        intentSet = false;
        iconContainer.SetFadeSpeed(GameSettings.INTENT_FADE_SPEED);
    }

    private void ActivateCollider(string _)
    {
        if (intentCollider != null)
            intentCollider.enabled = true;
    }
    private void DeactivateCollider(PointerData _)
    {
        if (intentCollider != null)
            intentCollider.enabled = false;
    }

    private void OnMouseEnter()
    {
        List<Tooltip> list = new List<Tooltip>();
        foreach (IntentIcon icon in GetComponentsInChildren<IntentIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        gameObject.GetComponentInParent<ITooltipSetter>().SetTooltip(list);
    }
    private void OnMouseExit()
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }

    private void OnDrawGizmos()
    {
        var scale = Vector3.one * iconContainer.transform.localScale.y;
        var scale2 = (scale + new Vector3(scale.x * 3, 0, 0)) * 1.05f;
        Gizmos.color = Color.cyan;
        GizmoExtensions.DrawBox(new Bounds(transform.position, scale), new Vector3(0, scale.y / 2, transform.position.z));
        Gizmos.color = Color.red;
        GizmoExtensions.DrawBox(new Bounds(transform.position, scale2), new Vector3(0, scale.y / 2, transform.position.z - 0.1f));
    }

    private void Update()
    {
        if (!intentSet && !askedForIntent) 
        {
            askForIntent();
        }
    }

    private void askForIntent() 
    {
        if (enemyId == string.Empty) 
        {
            return;
        }
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.EnemyIntents);
        askedForIntent = true;
    }

    private void onTurnChange(string whosTurn) 
    {
        if (whosTurn == "enemy")
        {
            iconContainer.ClearIcons();
            askedForIntent = false;
        }
        else if (!askedForIntent) 
        {
            askForIntent();
        }
    }

    private void OnUpdateIntent(EnemyIntent newIntent) 
    {
        if (newIntent.id != enemyId) return;

        iconContainer.ClearIcons();

        Dictionary<EnemyIntent.Intent, int> intentMap = new Dictionary<EnemyIntent.Intent, int>();
        foreach (var intent in newIntent.intents) 
        {
            var key = intentMap.Keys.FirstOrDefault(i => i.type == intent.type && i.value == intent.value);
            if (key != null)
            {
                intentMap[key]++;
            }
            else 
            {
                intentMap[intent] = 1;
            }
        }
        foreach (var intentItem in intentMap) 
        {
            var intent = intentItem.Key;
            int count = intentItem.Value;

            GameObject icon = Instantiate(iconPrefab);
            var intentIcon = icon.GetComponent<IntentIcon>();
            intentIcon.Initialize();
            var intentType = intentFromString(intent.type);
            intentIcon.SetValue(intentType == ENEMY_INTENT.attack ? intent.value : 0, count);
            intentIcon.SetIcon(intentType, intent.value);
            intentIcon.SetTooltip(intent.type, intentType != ENEMY_INTENT.unknown ? intent.description : "Unknown");

            iconContainer.AddIcon(icon);
        }
        iconContainer.CreateSprites();
        transform.localScale = Vector3.zero;
        transform.DOScale(1, GameSettings.INTENT_FADE_SPEED);
        askedForIntent = false;
        intentSet = true;

        intentCollider.enabled = true;
        intentCollider.offset = iconContainer.Bounds.center + iconContainer.transform.localPosition;
        intentCollider.size = iconContainer.Bounds.size;
    }

    private ENEMY_INTENT intentFromString(string value) 
    {
        value = value.ToLower().Trim();
        switch (value)
        {
            case "attack":
            case "attacking":
                return ENEMY_INTENT.attack;
            case "defend":
            case "defending":
                return ENEMY_INTENT.defend;
            case "plot":
            case "buff":
            case "plotting":
                return ENEMY_INTENT.plot;
            case "scheme":
            case "schemeing":
            case "debuff":
                return ENEMY_INTENT.scheme;
            case "stun":
            case "nothing":
            case "stunned":
                return ENEMY_INTENT.stunned;
            case "unknown":
            default:
                return ENEMY_INTENT.unknown;
        }
    
    }

}
