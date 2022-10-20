using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAnimationManager : MonoBehaviour
{
    [SerializeField] TextEffectManager textEffectManager;
    [SerializeField] ParticleSystem healEffect;

    [SerializeField] bool testRun;

    string entityId;


    private void Update()
    {
        if (testRun)
        {
            testRun = false;
            OnHeal(entityId, 20);
        }
    }

    void Start()
    {
        entityId = Utils.FindEntityId(gameObject);

        if (entityId == "unknown")
        {
            Debug.LogError(
                $"[HealAnimationManager] An enemy/player could not be found. This is on the [{gameObject.name}] object which is a child of [{transform.parent.name}].");
        }

        GameManager.Instance.EVENT_HEAL.AddListener(OnHeal);
    }

    protected virtual void OnHeal(string who, int healAmount)
    {
        // Check if me
        if (entityId != who) return;

        // Run Animation
        StartCoroutine(HealthAnimation(healAmount));
    }

    protected IEnumerator HealthAnimation(int hp)
    {
        healEffect.Play();
        yield return new WaitForSeconds(0.5f);
        textEffectManager.RunAnimation($"+{hp} HP");
    }
}