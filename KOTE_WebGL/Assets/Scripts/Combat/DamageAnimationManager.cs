using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAnimationManager : MonoBehaviour
{
    [SerializeField]
    TextEffectManager textEffectManager;

    [SerializeField]
    bool testRun;

    string entityId;


    private void Update()
    {
        if (testRun)
        {
            testRun = false;
            StartCoroutine(Animation(-20, -5, true));
        }
    }

    void Start()
    {
        entityId = transform.parent.gameObject.GetComponentInParent<EnemyManager>()?.EnemyData?.id ?? "player"; // transform.parent.gameObject.GetComponentInChildren<PlayerManager>()?.PlayerData.id;

        GameManager.Instance.EVENT_DAMAGE.AddListener(onDamage);
    }

    void onDamage(CombatTurnData.Target data)
    {
        // Check if me
        if (entityId != data.targetId && !(entityId == "player" && entityId == data.targetType)) return;

        // Run Animation
        if (data.healthDelta < 0 || data.defenseDelta != 0) 
        {
            StartCoroutine(Animation(data.healthDelta, data.defenseDelta, 
                data.defenseDelta != 0 && data.finalDefense == 0));
        }
    }

    IEnumerator Animation(int damage, int shieldDelta, bool breakShield)
    {
        if (damage != 0)
        {
            textEffectManager.RunAnimation($"{damage} HP");
        }
        else 
        {
            textEffectManager.RunAnimation($"Defended");
        }
        yield return new WaitForSeconds(0.1f);
    }
}
