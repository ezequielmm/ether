using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Combat.VFX;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

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
    public EnemyData enemy;
    public bool setEnemy = false;

    [SerializeField] private List<GameObject> enemyMap;

    public event Action<string, Vector3, GameObject> OnSignatureMoveReproduce;
    
    private EnemyData enemyData;
    private EnemyPrefab enemyPlacementData;
    private GameObject activeEnemy;

    public SpineAnimationsManagement spine;
    private Action RunWithEvent;
    private bool CalledEvent;
    private List<Guid> runningEvents = new List<Guid>();

    new private Collider2D collider;
    private Bounds enemyBounds;
    private StatusManager statusManager;
    private SkeletonRenderTextureFadeout spineFadeout;
    private CharacterSound characterSound;

    public static Dictionary<string, GameObject> InstantiatedEnemiesCache = new Dictionary<string, GameObject>();
    public static Dictionary<string, List<Action<GameObject>>> ThePainTrain = new Dictionary<string, List<Action<GameObject>>>();

    [SerializeField] private EnemiesConfig enemiesConfig;
    [SerializeField] private Animator animator;

    private bool awaitForEnemyRequestPrefab;

    public event Action OnEnemyDied;

    private string signature_move_name;
    
    public EnemyData EnemyData
    {
        set { enemyData = ProcessNewData(enemyData, value); }
        get { return enemyData; }
    }
    
    [SerializeField] public VFXList vfxList;
    
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
        if (!awaitForEnemyRequestPrefab && activeEnemy == null)
        {
            SetEnemyPrefab(current);
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
            SetEnemyPrefab(enemy);
        }
    }

    private void SetEnemyPrefab(EnemyData enemy)
    {
        var currentPrefab = GetComponentInChildren<EnemyPrefab>();
        if (currentPrefab != null)
            Destroy(currentPrefab.gameObject);

        var enemyName = enemiesConfig.GetEnemy(enemy.name);
        
        awaitForEnemyRequestPrefab = true;
        LoadEnemyPrefab(enemyName, (instance) =>
        {
            activeEnemy = instance;
            var originalScale = activeEnemy.transform.localScale;
            activeEnemy.transform.SetParent(transform);
            activeEnemy.transform.localPosition = Vector3.zero;
            activeEnemy.transform.localScale = originalScale;
            GrabEnemyFadeout(activeEnemy);
            enemyPlacementData = activeEnemy.GetComponentInChildren<EnemyPrefab>();
            enemyPlacementData.InitEnemy(enemy, FitTopAndBottomBar);
            // Add the cursorEnter and Exit for tooltips
            // Set mounting points

            this.enemy = enemy;
            
            collider = activeEnemy.GetComponentInChildren<Collider2D>();
            enemyBounds = collider.bounds;
            collider.enabled = false;
            
            gameObject.name = enemyName;
            
            Instantiate();

            awaitForEnemyRequestPrefab = false;
        });
        
        
    }

    private void FitTopAndBottomBar()
    {
        Vector3 top = enemyPlacementData.intentMountingPoint.position;
        top.y = Mathf.Min(GameSettings.INTENT_MAX_HEIGHT, top.y);
        TopBar.position = top;
        Vector3 bottom = enemyPlacementData.healthMountingPoint.position;
        bottom.y = Mathf.Max(transform.position.y, bottom.y);
        BottomBar.position = bottom;
    }

    private void LoadEnemyPrefab(string enemyName, Action<GameObject> onSuccess)
    {
        if (InstantiatedEnemiesCache.ContainsKey(enemyName))
        {
            onSuccess?.Invoke(Instantiate(InstantiatedEnemiesCache[enemyName]));
        }
        else
        {
            if (!ThePainTrain.ContainsKey(enemyName))
                ThePainTrain.Add(enemyName, new List<Action<GameObject>>());

            ThePainTrain[enemyName].Add(onSuccess);
            
            StartCoroutine(Load(enemyName));   
        }
    }
    IEnumerator Load(string enemyName)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(enemyName);
        while (!handle.IsDone)
            yield return null;
        
        if (!InstantiatedEnemiesCache.ContainsKey(enemyName))
            InstantiatedEnemiesCache.Add(enemyName, handle.Result);
        
        foreach (var callback in ThePainTrain[enemyName])
        {
            callback?.Invoke(Instantiate(handle.Result));
        }
        ThePainTrain[enemyName].Clear();
    }

    public void PlayAnimations(CombatTurnData combatTurnData)
    {
        if (combatTurnData.originId != enemyData.id) return;
        
        float afterEvent = 0;
        float length = 0;
        RunAfterTime(0.1f, () => { CalledEvent = false; });
        RunAfterEvent(() =>
        {
            GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(combatTurnData);
            runningEvents.Remove(combatTurnData.attackId);
        });
        
        // if (!string.IsNullOrEmpty(combatTurnData.action?.name) && combatTurnData.action.name == "signature_move")
        //     OnSignatureMoveReproduce?.Invoke(
        //         string.IsNullOrEmpty(signature_move_name) ? "Signature Attack" : signature_move_name, 
        //         enemyPlacementData.intentMountingPoint.position,
        //         activeEnemy);
        
        PlayAnimation(DetermineAnimation(combatTurnData.action),"");
    }
    
    private string DetermineAnimation(CombatTurnData.QueueActionData action)
        => action != null && !string.IsNullOrEmpty(action.name) ? action.name :
            action != null && !string.IsNullOrEmpty(action.hint) ? action.hint : "";
    
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
            waitDuration += PlayAnimation(attack.action?.name, attack.action != null && !string.IsNullOrEmpty(attack.action.hint) ? attack.action.hint : "Hit");
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
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(PlayAnimations);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(ActivateCollider);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(DeactivateCollider);

        GameManager.Instance.EVENT_UPDATE_INTENT.AddListener(OnUpdateIntent);

        Canvas canvas = barFader.GetComponent<Canvas>();
        canvas.sortingOrder = 1;
        
    }

    private void OnUpdateIntent(EnemyIntent intent)
    {
        // TODO: Workaround for starting with hidden idle
        // if (enemyData.name == "Cave Goblin" && intent.intents.Any(i => i.type == "special") && !spine.CurrentAnimationSequenceContains("hidden_idle"))
        //     spine.PlayAnimationSequence("hidden_idle");
        if (EnemyData.id != intent.id) return;
        
        signature_move_name = intent.intents.FirstOrDefault(e => e.type == "signature")?.name;
    }

    public static void ClearCache()
    {
        // foreach (var kv in InstantiatedEnemiesCache)
        // {
        //     Addressables.Release(kv.Value);
        // }
        // InstantiatedEnemiesCache.Clear();
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
            statusManager ??= GetComponentInChildren<StatusManager>();
            
            spine = activeEnemy.GetComponentInChildren<SpineAnimationsManagement>();
            
            // Suplant animator
            var vfxAnimations = spine.gameObject.AddComponent<Animator>();
            vfxAnimations.runtimeAnimatorController = animator.runtimeAnimatorController;
            Destroy(animator);
            
            spine.Init(new EnemyIdleSolver(statusManager), vfxList);
            
            statusManager.OnStatusUpdated += (statuses) => spine.PlayIdle();
            
            spine.ANIMATION_EVENT.AddListener(OnAnimationEvent);
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

    public float PlayAnimation(string animationSequence, string fallbackAnimation, Action doWhenAnimationEnds = null)
    {
        string sequence = CheckAnimationName(animationSequence, fallbackAnimation);
        float length = spine.PlayAnimationSequence(sequence);
        characterSound?.PlaySound(fallbackAnimation);

        if (animationSequence == "signature_move")
            OnSignatureMoveReproduce?.Invoke(signature_move_name, transform.position, activeEnemy);
        
        if (doWhenAnimationEnds != null)
            RunAfterTime(length, doWhenAnimationEnds);
        
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
            PlayAnimation("death", "Death", () =>
            {
                spineFadeout.OnFadeoutComplete += DestroyOnFadeout;
                spineFadeout.enabled = true;
                OnEnemyDied?.Invoke();
            });
            barFader.FadeOutUi();
        }
    }

    public void AddActionWhenDied(Action toRun)
    {
        OnEnemyDied += toRun;
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
            RunWithEvent?.Invoke();
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
}