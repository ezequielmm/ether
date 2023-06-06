using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour, ITooltipSetter
{
    public CanvasGroupFade barFader;
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

    [SerializeField] private List<GameObject> enemyMap;
    
    private EnemyData enemyData;
    private EnemyPrefab enemyPlacementData;
    private GameObject activeEnemy;

    private SpineAnimationsManagement spine;
    private Action RunWithEvent;
    private bool CalledEvent;
    private List<Guid> runningEvents = new List<Guid>();

    new private Collider2D collider;
    private Bounds enemyBounds;
    private StatusManager statusManager;
    private SkeletonRenderTextureFadeout spineFadeout;
    private CharacterSound characterSound;


    public EnemyData EnemyData
    {
        set { enemyData = ProcessNewData(enemyData, value); }
        get { return enemyData; }
    }

    public void SetEnemeyData(EnemyData data)
    {
        if (enemyData != null)
        {
            Debug.LogWarning($"[EnemyManager] Overwriting exisiting Enemy Data.");
        }

        enemyData = data;
        ProcessNewData(null, enemyData);
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
            GrabEnemyFadeout(activeEnemy);
            enemyPlacementData = activeEnemy.GetComponentInChildren<EnemyPrefab>();
            enemyPlacementData.FitColliderToArt();
            // Add the cursorEnter and Exit for tooltips
            // Set mounting points
            Vector3 top = enemyPlacementData.intentMountingPoint.position;
            top.y = Mathf.Min(GameSettings.INTENT_MAX_HEIGHT, top.y);
            TopBar.position = top;
            Vector3 bottom = enemyPlacementData.healthMountingPoint.position;
            bottom.y = Mathf.Max(transform.position.y, bottom.y);
            BottomBar.position = bottom;

            collider = activeEnemy.GetComponentInChildren<Collider2D>();
            enemyBounds = collider.bounds;
            collider.enabled = false;

            this.enemyType = enemyType.ParseToEnum<EnemyTypes>();
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
            if (attack.action == null || string.IsNullOrEmpty(attack.action.hint))
            {
                endCalled = true;
                GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
            }
            // Run Attack Animation Or Status effects
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.damage))
            {
                runningEvents.Add(attack.attackId);
                // Run Attack
                Debug.Log("+++++++++++++++[Enemy]Attack");

                var f = PlayAnimation(attack.action?.name, "Attack");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.defense)) // Defense Up
            {
                runningEvents.Add(attack.attackId);
                var f = PlayAnimation(attack.action?.name, "Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.heal)) // Health Up
            {
                runningEvents.Add(attack.attackId);
                var f = PlayAnimation(attack.action?.name, "Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
        }

        if (!endCalled)

        {
            runningEvents.Add(attack.attackId); // If no conditions met, pass onto the target and play cast
            var f = PlayAnimation(attack.action?.name, "Cast");
            if (f > afterEvent) afterEvent = f;
            endCalled = true;
            RunAfterEvent(() =>
            {
                GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                runningEvents.Remove(attack.attackId);
            });
        }
        else if (afterEvent > 0)
        {
            RunAfterTime(afterEvent, () =>
            {
                if (RunWithEvent != null && !CalledEvent && runningEvents.Contains(attack.attackId))
                {
                    Debug.LogWarning(
                        $"[EnemyManager | {enemyData.name}] Animation is missing a 'attack' or 'release' event!");
                    RunWithEvent.Invoke();
                    runningEvents.Remove(attack.attackId);
                }
            });
        }
    }

    private void OnAttackResponse(CombatTurnData attack)
    {
        var target = attack.GetTarget(enemyData.id);
        if (attack.originId == enemyData.id) PlaySound(attack);
        if (target == null) return;

        Debug.Log($"[EnemyManager] Combat Response GET!");

        // Negitive Deltas
        float waitDuration = 0;
        if (target.defenseDelta < 0 || target.healthDelta < 0)
        {
            GameManager.Instance.EVENT_DAMAGE.Invoke(target);
        }

        if (target.healthDelta < 0) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            waitDuration += PlayAnimation(attack.action?.name, "Hit");
        }

        if (target.healthDelta > 0) // Healed!
        {
            // Play Rising Chimes
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

    private void PlaySound(CombatTurnData attack)
    {
        foreach (var target in attack.targets)
        {
            if (target.defenseDelta < 0 &&
                target.healthDelta >= 0) // Hit and defence didn't fall or it did and no damage
            {
                // Play Armored Clang
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.EnemyDefensive, /*enemyData.name +*/ "Block");
            }
            else if (target.healthDelta < 0) // Damage Taken no armor
            {
                // Play Attack audio
                // Can be specific, but we'll default to "Attack"
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.EnemyOffensive, enemyData.name + "Attack");
            }

            // Positive Deltas
            if (target.defenseDelta > 0) // Defense Buffed
            {
                // Play Metallic Ring
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.EnemyDefensive, enemyData.name + "Cast");
            }

            if (target.healthDelta > 0) // Healed!
            {
                // Play Rising Chimes
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.EnemyOffensive, enemyData.name + "Cast");
            }
        }
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

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(ActivateCollider);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(DeactivateCollider);

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
            spine = activeEnemy.GetComponentInChildren<SpineAnimationsManagement>();
            spine.ANIMATION_EVENT.AddListener(OnAnimationEvent);
            spine.PlayAnimationSequence("Idle");
        }
        
        characterSound = activeEnemy.GetComponentInChildren<CharacterSound>();
    }

    private void OnUpdateEnemy(EnemyData newEnemyData)
    {
        if (newEnemyData.id == enemyData.id)
        {
            // healthBar.DOValue(newEnemyData.hpMin, 1);
            EnemyData = newEnemyData;
        }
    }

    private void OnDrawGizmos()
    {
        if (!string.IsNullOrEmpty(enemyData.size))
        {
            float size = Utils.GetSceneSize(enemyData.size.ParseToEnum<Size>());
            Gizmos.color = Color.cyan;
            GizmoExtensions.DrawBox(size, size * 2, (Vector3.up * size) + transform.position);
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
            healthBar.DOValue(current.Value, 1).OnComplete(() => CheckDeath(current.Value));
        }
    }

    public float PlayAnimation(string animationSequence, string fallbackAnimation)
    {
        string animationName = CheckAnimationName(animationSequence, fallbackAnimation);
        float length = spine.PlayAnimationSequence(animationName);
        spine.PlayAnimationSequence("Idle");
        characterSound?.PlaySound(fallbackAnimation);
        return length;
    }

    private string CheckAnimationName(string animationSequence, string defaultAnimation)
    {
        if (string.IsNullOrEmpty(animationSequence))
        {
            Debug.LogWarning(
                $"[EnemyManager] Warning! enemy {enemyData.name} received a null or empty animation request");
            return defaultAnimation;
        }

        return animationSequence.ToLower();
    }

    private float OnDeath()
    {
        float length = spine.PlayAnimationSequence("Death");
        characterSound?.PlaySound("Death");
        return length;
    }

    private void ActivateCollider(PointerData _)
    {
        if (collider != null)
            collider.enabled = true;
    }

    private void DeactivateCollider(string _)
    {
        if (collider != null)
            collider.enabled = false;
    }

    private void CheckDeath(int current)
    {
        // if (enemyData.hpCurrent < 1)//TODO: enemyData is not up to date
        if (current < 1)
        {
            // Tell game that a player is dying
            GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), nameof(EnemyState.dying));

            explodePS.transform.parent = null;
            explodePS.Play();

            // Play animation
            RunAfterTime(OnDeath(), () =>
            {
                spineFadeout.OnFadeoutComplete += DestroyOnFadeout;
                spineFadeout.enabled = true;
            });
            barFader.FadeOutUi();
        }
    }

    private void DestroyOnFadeout(SkeletonRenderTextureFadeout target)
    {
        // Tell game that a player is dead
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), nameof(EnemyState.dead));
        Destroy(explodePS.gameObject);
        Destroy(this.gameObject);
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
        if (enemy != null) return enemy;

        Debug.LogError($"[EnemyManager] Missing {enemyName} prefab. Using SporeMonger as a backup");
        enemyName = "SporeMonger";
        enemy = FindPrefab(enemyName);
        if (enemy != null) return enemy;
        Debug.LogError($"[EnemyManager] Missing {enemyName} prefab.");
        return null;
    }

    private void GrabEnemyFadeout(GameObject enemy)
    {
        spineFadeout = enemy.GetComponentInChildren<SkeletonRenderTextureFadeout>();
    }

    public void SetTooltip(List<Tooltip> tooltips)
    {
        collider.enabled = true;
        enemyBounds = collider.bounds;
        Vector3 anchorPoint = new Vector3(enemyBounds.center.x - enemyBounds.extents.x,
            enemyBounds.center.y, 0);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.MiddleRight, anchorPoint,
            null);
        collider.enabled = false;
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