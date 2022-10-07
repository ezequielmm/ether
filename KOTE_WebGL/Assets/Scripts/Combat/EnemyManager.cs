using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    private EnemyData enemyData;
    public ParticleSystem hitPS;
    public ParticleSystem explodePS;
    public Slider healthBar;
    public TMP_Text healthTF;
    public DefenseController defenseController;
    public Transform TopBar;
    public Transform BottomBar;

    public bool overrideEnemy = false;
    public EnemyTypes enemyType;
    public bool setEnemy = false;

    [SerializeField]
    private List<GameObject> enemyMap;
    private EnemyPrefab enemyPlacementData;
    private GameObject activeEnemy;

    private SpineAnimationsManagement spine;
    private Action RunWithEvent;
    private bool CalledEvent;

    new private Collider2D collider;
    private StatusManager statusManager;


    public EnemyData EnemyData { 
        set
        {
            enemyData = ProcessNewData(enemyData, value);
        }
        get
        {
            return enemyData;
        }
    }


    private EnemyData ProcessNewData(EnemyData old, EnemyData current)
    {
        if (activeEnemy == null)
        {
            SetEnemyPrefab(current.name);
        }

        if (old == null)
        {
            SetDefense(current.defense);
            SetHealth(current.hpCurrent, current.hpMax);
            return current;
        }

        SetDefense(current.defense);
        SetHealth(current.hpCurrent, current.hpMax);

        return current;
    }

    private void Update()
    {
        if (setEnemy) 
        {
            setEnemy = false;
            SetEnemyPrefab(Enum.GetName(typeof(EnemyTypes), enemyType));
        }
    }

    private void SetEnemyPrefab(string enemyType)
    {
        var currentPrefab = GetComponentInChildren<EnemyPrefab>();
        if (currentPrefab != null)
            Destroy(currentPrefab.gameObject);
        var prefab = GetEnemyPrefab(enemyType);
        if (prefab != null)
        {
            activeEnemy = Instantiate(prefab, transform);
            activeEnemy.transform.localPosition = Vector3.zero;
            enemyPlacementData = activeEnemy.GetComponent<EnemyPrefab>();
            enemyPlacementData.FitColliderToArt();
            // Add the cursorEnter and Exit for tooltips
            // Set mounting points
            Vector3 top = enemyPlacementData.intentMountingPoint.position;
            top.y = Mathf.Min(GameSettings.INTENT_MAX_HEIGHT, top.y);
            TopBar.position = top;
            Vector3 bottom = enemyPlacementData.healthMountingPoint.position;
            bottom.y = Mathf.Max(transform.position.y, bottom.y);
            BottomBar.position = bottom;

            collider = activeEnemy.GetComponent<Collider2D>();

            // Set tooltip events
            enemyPlacementData.onCursorEnter.AddListener(SetTooltip);
            enemyPlacementData.onCursorExit.AddListener(SemoveTooltip);


            this.enemyType = Utils.ParseEnum<EnemyTypes>(enemyType);
            gameObject.name = Enum.GetName(typeof(EnemyTypes), this.enemyType);

            Instantiate();
        }
    }

    private void OnAttackRequest(CombatTurnData attack)
    {
        // TODO: Ensure that the player sets the correct enemy when attacked.
        if (attack.originId != enemyData.id) return;

        Debug.Log($"[EnemyManager] Combat Request GET!");

        bool endCalled = false;
        float afterEvent = 0;
        RunAfterTime(0.1f, () => { CalledEvent = false; });
        foreach (CombatTurnData.Target target in attack.targets)
        {
            // Run Attack Animation Or Status effects
            if (target.effectType == nameof(ATTACK_EFFECT_TYPES.damage))
            {
                // Run Attack
                var f = Attack();
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.defense)) // Defense Up
            {
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.heal)) // Health Up
            {
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
        }
        if (!endCalled)
        { // If no conditions met, pass onto the target and play cast
            var f = PlayAnimation("Cast");
            if (f > afterEvent) afterEvent = f;
            endCalled = true;
            RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
        }
        else if (afterEvent > 0) 
        {
            RunAfterTime(afterEvent, () => 
            {
                if (RunWithEvent != null && !CalledEvent) 
                {
                    Debug.LogWarning($"[EnemyManager | {enemyData.name}] Animation is missing a 'attack' or 'release' event!");
                    RunWithEvent.Invoke();
                }
            });
        }
    }
    private void OnAttackResponse(CombatTurnData attack)
    {
        var target = attack.GetTarget(enemyData.id);
        if (target == null) return;

        Debug.Log($"[EnemyManager] Combat Response GET!");

        // Negitive Deltas
        float waitDuration = 0;
        if (target.defenseDelta < 0 || target.healthDelta < 0) {
            GameManager.Instance.EVENT_DAMAGE.Invoke(target);
        }

        if (target.defenseDelta < 0 && target.healthDelta >= 0) // Hit and defence didn't fall or it did and no damage
        {
            // Play Armored Clang
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Block");
        }
        else if (target.healthDelta < 0) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Attack");
            waitDuration += OnHit();
        }

        // Positive Deltas
        if (target.defenseDelta > 0) // Defense Buffed
        {
            // Play Metallic Ring
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Up");
        }
        if (target.healthDelta > 0) // Healed!
        {
            // Play Rising Chimes
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Heal");
            GameManager.Instance.EVENT_HEAL.Invoke(EnemyData.id, target.healthDelta);
            waitDuration += 1;
        }

        // Update the UI
        if (target.defenseDelta != 0)
        {
            SetDefense(target.finalDefense);
        }
        if (target.healthDelta != 0)
        {
            SetHealth(target.finalHealth);
        }

        // Add status changes
        if (target.statuses != null)
        {
            statusManager.UpdateStatus(target.statuses);
        }

        RunAfterTime(waitDuration, () => GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(attack.attackId));
    }

    private void SetDefense(int? value = null)
    {
        if (value == null)
        {
            value = enemyData.defense;
        }
        defenseController.Defense = value.Value;
    }

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(OnAttackRequest);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);

        statusManager = GetComponentInChildren<StatusManager>();
    }

    private void Instantiate() 
    {
        // Grab first spine animation management script we find. This is a default. We'll set this when spawning the enemy usually.
        if (activeEnemy == null)
        {
            activeEnemy = GetComponentInChildren<SpineAnimationsManagement>()?.gameObject;
            if (activeEnemy == null)
            {
                Debug.LogWarning($"[Enemy Manager] Could not find enemy animation");
            }
        }

        if (activeEnemy != null) 
        {
            spine = activeEnemy.GetComponent<SpineAnimationsManagement>();
            spine.ANIMATION_EVENT.AddListener(OnAnimationEvent);
            spine.PlayAnimationSequence("Idle");
        }
        
    }

    private void OnUpdateEnemy(EnemyData newEnemyData)
    {
        if (newEnemyData.enemyId == enemyData.enemyId)
        {
            // healthBar.DOValue(newEnemyData.hpMin, 1);
            EnemyData = newEnemyData;
        }
    }

    private void OnDrawGizmos()
    {
        if (!string.IsNullOrEmpty(enemyData.size))
        {
            float size = Utils.GetSceneSize(Utils.ParseEnum<Size>(enemyData.size));
            Gizmos.color = Color.cyan;
            Utils.GizmoDrawBox(size, size * 2, (Vector3.up * size) + transform.position);
        }
    }

    public void SetHealth(int? current = null, int? max = null)
    {
        if (current == null)
        {
            current = enemyData.hpCurrent;
        }
        if (max == null)
        {
            max = enemyData.hpMax;
        }
        Debug.Log($"[EnemyManager] Health: {current}/{max}");

        healthTF.SetText($"{current}/{max}");

        healthBar.maxValue = max.Value;

        if (healthBar.value != current)
        {
            hitPS.Play();
            healthBar.DOValue(current.Value, 1).OnComplete(()=>CheckDeath(current.Value));
        }
    }

    public float PlayAnimation(string animationSequence)
    {
        float length = spine.PlayAnimationSequence(animationSequence);
        spine.PlayAnimationSequence("Idle");
        return length;
    }

    private float Attack() 
    {
        Debug.Log("+++++++++++++++[Enemy]Attack");

        float length = spine.PlayAnimationSequence("Attack");
        spine.PlayAnimationSequence("Idle");
        return length;
    }

    private float OnHit()
    {
        float length = spine.PlayAnimationSequence("Hit");
        spine.PlayAnimationSequence("Idle");
        return length;

    }

    private float OnDeath()
    {
        float length = spine.PlayAnimationSequence("Death");
        return length;
    }

    private void CheckDeath(int current)
    {
       // if (enemyData.hpCurrent < 1)//TODO: enemyData is not up to date
        if (current  < 1)
        {
            // Tell game that a player is dying
            GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), nameof(EnemyState.dying));

            explodePS.transform.parent = null;
            explodePS.Play();

            // Play animation
            RunAfterTime(OnDeath(), () => {
                // Tell game that a player is dead
                GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), nameof(EnemyState.dead));
                Destroy(explodePS.gameObject);
                Destroy(this.gameObject);
            });
        }
    }

    private void RunAfterEvent(Action toRun)
    {
        RunWithEvent = toRun;
    }
    private void OnAnimationEvent(string eventName)
    {
        if (eventName.Equals("attack") || eventName.Equals("release"))
        {
            CalledEvent = true;
            RunWithEvent.Invoke();
        }
    }
    private void RunAfterTime(float time, Action toRun)
    {
        StartCoroutine(runCoroutine(time, toRun));
    }
    private IEnumerator runCoroutine(float time, Action toRun)
    {
        yield return new WaitForSeconds(time);
        toRun.Invoke();
    }

    private GameObject GetEnemyPrefab(string enemyName) 
    {
        if (overrideEnemy) 
        {
            enemyName = enemyType.ToString();
        }
        var enemy = FindPrefab(enemyName);
        if(enemy != null) return enemy;
        Debug.LogError($"[EnemyManager] Missing {enemyName} prefab. Using SporeMonger as a backup");
        enemyName = "SporeMonger";
        enemy = FindPrefab(enemyName);
        if (enemy != null) return enemy;
        Debug.LogError($"[EnemyManager] Missing {enemyName} prefab.");
        return null;
    }

    private List<Tooltip> GetTooltipInfo()
    {
        List<Tooltip> list = new List<Tooltip>();

        foreach (IntentIcon icon in GetComponentsInChildren<IntentIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        foreach (StatusIcon icon in GetComponentsInChildren<StatusIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        return list;
    }
    private void SetTooltip()
    {
        Vector3 anchorPoint = new Vector3(collider.bounds.center.x - collider.bounds.extents.x,
            collider.bounds.center.y, 0);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(GetTooltipInfo(), TooltipController.Anchor.MiddleRight, anchorPoint, null);
    }
    private void SemoveTooltip()
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }

    private GameObject FindPrefab(string enemyName)
    {
        foreach (var enemy in enemyMap)
        {
            if (enemy.name.ToLower().Equals(enemyName.ToLower()))
            {
                return enemy;
            }
        }
        return null;
    }
}