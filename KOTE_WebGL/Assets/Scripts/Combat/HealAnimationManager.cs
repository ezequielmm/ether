using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAnimationManager : MonoBehaviour
{
    [SerializeField]
    TextEffectManager textEffectManager;
    [SerializeField]
    ParticleSystem healEffect;

    [SerializeField]
    bool testRun;

    string entityId;


    private void Update()
    {
        if (testRun) 
        {
            testRun = false;
            onHeal(entityId, 20);
        }
    }

    void Start()
    {
        entityId = transform.parent.gameObject.GetComponentInChildren<EnemyManager>()?.EnemyData?.id ?? "player"; // transform.parent.gameObject.GetComponentInChildren<PlayerManager>()?.PlayerData.id;
        
        GameManager.Instance.EVENT_HEAL.AddListener(onHeal);
    }

    void onHeal(string who, int healAmount) 
    {
        // Check if me
        if (entityId != who) return;

        // Run Animation
        StartCoroutine(healthAnimation(healAmount));
    }

    IEnumerator healthAnimation(int hp) 
    {
        healEffect.Play();
        yield return new WaitForSeconds(0.5f);
        textEffectManager.RunAnimation($"+{hp} HP");
    }
}
