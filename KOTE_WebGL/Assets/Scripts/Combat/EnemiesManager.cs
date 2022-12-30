using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EnemiesManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float extent = 4.44f;
    public float floor = -1.5f;
    public float spawnX = 5;
    List<GameObject> enemies = new List<GameObject>();
    Vector3 spawnPos => new Vector3(spawnX, floor, 0);



    private void Awake()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener(OnEnemiesUpdate);
        GameManager.Instance.EVENT_ADD_ENEMIES.AddListener(OnAddEnemies);
    }

    private EnemyManager SpawnEnemy(EnemyData enemyData)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, this.transform);
        newEnemy.transform.localPosition = spawnPos;
        EnemyManager em = newEnemy.GetComponent<EnemyManager>();
        em.EnemyData = enemyData;
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
        return Utils.GetSceneSize(Utils.ParseEnum<Size>(size));
    }

    private void OnEnemiesUpdate(EnemiesData enemiesData)
    {
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
                enemyManager = SpawnEnemy(data);
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
     
        for(int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!newEnemyList.Contains(enemy)) 
            {
                enemies.RemoveAt(i);
                if (enemy != null)
                {
                    enemy.transform.DOMove(transform.position + Vector3.right * spawnX, 1).OnComplete(() => { Destroy(enemy); });
                }
                i--;
                continue;
            }
        }
        enemies.Clear();
        enemies = newEnemyList;

        PositionEnemies();
    }

    private void OnAddEnemies(EnemiesData enemiesData)
    {
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
                enemyManager = SpawnEnemy(data);
                enemies.Add(enemyManager.gameObject);
            }
        }
        PositionEnemies();
    }

    private void PositionEnemies() 
    {
        float width = 0;
        foreach (var enemyObj in enemies) 
        {
            var enemy = enemyObj.GetComponent<EnemyManager>();
            width += GetSize(enemy.EnemyData.size); // enemy.collider.bounds.size.x;
        }
        float spillOver = Mathf.Max(0, width - (extent * 2));
        float spacing = Mathf.Max(0, (extent * 2) - width) / (enemies.Count);
        float leftEdge = transform.position.x + -(width / 2f) + -spillOver + -spacing/2;
        StringBuilder sb = new StringBuilder();

        foreach (var enemyObj in enemies)
        {
            var enemy = enemyObj.GetComponent<EnemyManager>();
            sb.Append($"[{enemy.EnemyData.enemyId} | {enemy.EnemyData.id}], ");
            float size = GetSize(enemy.EnemyData.size); //enemy.collider.bounds.size.x;
            float xPos = leftEdge + size / 2;
            if (enemies.Count <= 1)
            {
                xPos = transform.position.x;
            }
            Vector3 desiredPosition = new Vector3(xPos, transform.position.y + floor, transform.position.z);
            enemy.transform.DOMove(desiredPosition, 1);
            leftEdge += size + spacing;
        }
        Debug.Log($"[EnemiesManager] List of enemy IDs: {sb.ToString().Substring(0, sb.ToString().Length-2)}");
    }

    private void OnDrawGizmosSelected()
    {
        Bounds b = new Bounds(transform.position, new Vector3(extent * 2, 5, 0));
        Gizmos.color = Color.green;
        GizmoExtensions.DrawBox(b);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(transform.position.x - 4, transform.position.y + floor, 0), new Vector3(transform.position.x + 4, transform.position.y + floor, 0));
    }
}
