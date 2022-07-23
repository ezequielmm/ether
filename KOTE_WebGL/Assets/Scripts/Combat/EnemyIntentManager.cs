using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIntentManager : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;
    EnemyManager enemyManager;

    string enemyId => enemyManager.EnemyData.id;

    
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
    }

    private void onTurnChange(string whosTurn) 
    {
        if (whosTurn == "enemy") 
        {
            iconContainer.ClearIcons();
        }
    }

    private void OnUpdateIntent(EnemyIntent newIntent) 
    {
        if (newIntent.id != enemyId) return;

        iconContainer.ClearIcons();

        Dictionary<EnemyIntent.Intent, int> intentMap = new Dictionary<EnemyIntent.Intent, int>();
        foreach (var intent in newIntent.intents) 
        {
            if (intentMap.ContainsKey(intent))
            {
                intentMap[intent]++;
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
            intentIcon.SetValue(intent.value, count);
            intentIcon.SetIcon(intentFromString(intent.type), intent.value);
            intentIcon.SetTooltip(intent.description);

            iconContainer.AddIcon(icon);
            iconContainer.ReorganizeSprites();
        }
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
