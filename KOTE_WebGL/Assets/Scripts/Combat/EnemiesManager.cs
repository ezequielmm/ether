using System;
using System.Collections.Generic;
using System.Linq;
using Combat;
using UnityEngine;
using DG.Tweening;

public class EnemiesManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    
    public List<LineData> lines = new()
    {
        new LineData()
        {
            extent = 4.44f,
            floor = -1.5f,
            zPosition = 0
        },
        new LineData()
        {
            extent = 4.44f,
            floor = -0.6f,
            zPosition = -2
        }
    };
    [Range(1f, 8f)]
    public float sampleEnemyWidth = 4.44f;
    [Range(1,5)]
    public int sampleEnemyCount = 3;

    public List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private UltiFeedback ultiFeedback;

    [Serializable]
    public class LineData
    {
        public float xOffset;
        public float extent;
        public float floor;
        public float zPosition;
        public float spacing = 2;
    }

    private EnemyManager SpawnEnemy(EnemyData enemyData, int lineIndex)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, this.transform);
        var spawnPos = new Vector3(newEnemy.transform.position.x, lines[lineIndex].floor, lines[lineIndex].zPosition);
        newEnemy.transform.localPosition = spawnPos;
        EnemyManager em = newEnemy.GetComponent<EnemyManager>();
        em.OnEnemySpawned += GameManager.Instance.EnemySpawned;
        em.OnEnemyDied += GameManager.Instance.EnemyDied;
        em.SetEnemeyData(enemyData);

        em.OnSignatureMoveReproduce += ultiFeedback.DoFeedback;
        
        return em;
    }

    private void OnEnable()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Enemies);
    }

    private bool EnemyExists(EnemyData data) 
    {
        for (int i = 0; i < enemies.Count; i++) 
        {
            var enemy = enemies[i];
            if (enemy == null) 
            {
                enemies.RemoveAt(i);
                i--;
                continue;
            }
            var other = enemy.GetComponent<EnemyManager>().EnemyData;
            if (other.id == data.id) 
            {
                return true;
            }
        }
        return false;
    }

    private EnemyManager GetEnemy(string id) 
    {
        foreach (var enemy in enemies) 
        {
            var other = enemy.GetComponent<EnemyManager>();
            if (other.EnemyData.id == id) 
            {
                return other;
            }
        }
        return null;
    }

    private float GetSize(string size) 
    {
        return Utils.GetSceneSize(size.ParseToEnum<Size>());
    }

    public void OnEnemiesUpdate(EnemiesData enemiesData)
    {
        Debug.Log($"[EnemiesManager] EnemiesUpdate");
        if (enemiesData.data.Count == 0)
        {
            GameManager.Instance.EVENT_GAME_STATUS_CONFIRM.Invoke();
        }
        
        List<GameObject> newEnemyList = new List<GameObject>();
        foreach (EnemyData data in enemiesData.data)
        {
            bool enemyExists = EnemyExists(data);
            EnemyManager enemyManager = null;
            if (enemyExists) 
            {
                enemyManager = GetEnemy(data.id);
                if (enemyManager.gameObject == null) 
                {
                    enemyExists = false;
                }
            }
            if (enemyExists)
            {
                enemyManager.EnemyData = data;
                newEnemyList.Add(enemyManager.gameObject);
            }
            else 
            {
                enemyManager = SpawnEnemy(data, data.line ?? 0);
                enemies.Add(enemyManager.gameObject);
                newEnemyList.Add(enemyManager.gameObject);
            }
        }

        // if there's no enemies, then combat is completed
        if (enemies.Count == 0)
        {
            GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), nameof(EnemyState.dead));
            return;
        }
     
        // for(int i = 0; i < enemies.Count; i++)
        // {
        //     var enemy = enemies[i];
        //     if (!newEnemyList.Contains(enemy)) 
        //     {
        //         enemies.RemoveAt(i);
        //         if (enemy != null)
        //         {
        //             enemy.transform.DOMove(transform.position + Vector3.right * spawnX, 1).OnComplete(() => { Destroy(enemy); });
        //         }
        //         i--;
        //         continue;
        //     }
        // }

        PositionEnemiesInLine();
    }
    
    public void OnTransformEnemies(EnemiesData enemiesData)
    {
        Debug.Log($"[EnemiesManager] OnTransformEnemies {enemies.Count}");
        var oldEnemyNewData = enemiesData.data[0];
        var oldEnemy = GetEnemy(oldEnemyNewData.id);
        var indexOfOldEnemy = enemies.IndexOf(oldEnemy.gameObject);
        enemies.Remove(oldEnemy.gameObject);

        var dataWithOnlyTransformation = new EnemiesData {
            data = new List<EnemyData>(enemiesData.data.Where(e => e.id != oldEnemy.EnemyData.id))
        };
        
        GameManager.Instance.ActiveEndOfTurnButton(false);
        Action<GameObject> spawnTransformationAction = (_) =>
        {
            Debug.Log($"[EnemiesManager] Transform done {enemies.Count}");
            OnAddEnemies(dataWithOnlyTransformation, true, indexOfOldEnemy);
            if (GameManager.Instance.IsPlayerTurn)
                GameManager.Instance.ActiveEndOfTurnButton(true);
        };
        
        // if the enemy is dead, then we need to wait for it to die before we can add the new enemies
        if (oldEnemyNewData.hpCurrent <= 0)
        {
            oldEnemy.AddActionWhenDied(spawnTransformationAction);
            return;
        }
        
        spawnTransformationAction(null);
    }
    
    public void OnAddEnemies(EnemiesData enemiesData, bool isTransformation = false, int indexToInsert = -1)
    {
        Debug.Log($"[EnemiesManager] OnAddEnemies");
        foreach (EnemyData data in enemiesData.data)
        {
            bool enemyExists = EnemyExists(data);
            EnemyManager enemyManager = null;
            if (enemyExists) 
            {
                enemyManager = GetEnemy(data.id);
                if (enemyManager.gameObject != null) 
                {
                    enemyManager.EnemyData = data;
                }
            }
            else
            {
                enemyManager = SpawnEnemy(data, data.line ?? 0);
                if (indexToInsert <= -1)
                    enemies.Add(enemyManager.gameObject);
                else
                    enemies.Insert(indexToInsert, enemyManager.gameObject);
            }
        }

        PositionEnemiesInLine(isTransformation);
    }

    [ContextMenu("PositionEnemiesInLine")]
    private void Position()
    {
        PositionEnemiesInLine();
    }
    
    private void PositionEnemiesInLine(bool isTransformation = false)
    {
        var enemiesManager = enemies.Select(e => e.GetComponent<EnemyManager>()).ToArray();
        PositionEnemies(enemiesManager,
            0, isTransformation);
        PositionEnemies(enemiesManager,
            1, isTransformation);
    }
    private void PositionEnemies(EnemyManager[] enemies, int lineIndex, bool isTransformaiton = false) 
    {
        // Total enemy width
        if (enemies.Length == 0) return;
        
        enemies = enemies.Where(e =>
        {
            var enemyLine = e.GetComponent<EnemyManager>().EnemyData.line;
            if (lineIndex == 0 && enemyLine == null)
                return true;
            return enemyLine == lineIndex;
        }).ToArray();

        Debug.Log($"positioning: {enemies.Length} enemies in line {lineIndex}");
        
        var lineData = lines[lineIndex];

        var leftEdge = -lineData.extent - lineData.xOffset;
        var enemiesWidth = enemies.Sum(e => GetSize(e.EnemyData.size));

        var lineTotalLength = lineData.extent * 2;
        var spacesCount = (enemies.Length - 1);
        
        var freeSpace = (lineTotalLength - enemiesWidth) / spacesCount;
        var spacing = Mathf.Min(freeSpace,  lineData.spacing);
        
        var currentOffset = spacing < lineData.spacing ? 0f  :
            (lineTotalLength - (enemiesWidth + spacing * spacesCount)) / 2 ;
        
        // Add all the enemies
        foreach (var enemy in enemies)
        {
            var currentX = leftEdge + currentOffset;
            float size = GetSize(enemy.EnemyData.size); //enemy.collider.bounds.size.x;
            
            Vector3 desiredPosition = new Vector3(currentX + (size / 2), transform.localPosition.y + lineData.floor, lineData.zPosition);
            
            // Enemy transform special handle
            if (!isTransformaiton)
            {
                enemy.transform.DOLocalMove(desiredPosition, 1);
            }
            else
            {
                enemy.transform.localPosition = desiredPosition;
                var initialScale = enemy.transform.localScale;
                enemy.transform.localScale = Vector3.zero;
                enemy.transform.DOScale(initialScale, 1f);
            }
            
            currentOffset += size + spacing;
        }
    }
}
