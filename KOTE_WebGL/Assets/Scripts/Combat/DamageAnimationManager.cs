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
        entityId = Utils.FindEntityId(gameObject);

        if (entityId == "unknown")
        {
            StartCoroutine(GetEntity());
        }

        GameManager.Instance.EVENT_DAMAGE.AddListener(onDamage);
    }

    IEnumerator GetEntity()
    {
        yield return null;
        entityId = Utils.FindEntityId(gameObject);
        for (int i = 0; i < 20; i++)
        {
            if (entityId == "unknown")
            {
                yield return null;
                entityId = Utils.FindEntityId(gameObject);
            }
            else
            {
                break;
            }
        }
        if (entityId == "unknown")
        {
            Debug.LogError($"[DamageAnimationManager] An enemy/player could not be found. This is on the [{gameObject.name}] object which is a child of [{transform.parent.name}].");
        }
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
