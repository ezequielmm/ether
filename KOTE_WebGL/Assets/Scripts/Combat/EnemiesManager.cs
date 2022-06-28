using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
  
    public List<GameObject> EnemiesPrefabs = new List<GameObject>();
   // public Dictionary<string, GameObject> EnemyPrefabsDictionary = new Dictionary<string, GameObject>();
    private List<GameObject> EnemiesGoArray = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
       
    }
    private void Awake()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener(OnEnemiesUpdate);
    }

    private void OnEnable()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Enemies);
    }

    private void OnEnemiesUpdate(EnemiesData enemiesData)
    {
        Debug.Log(enemiesData);
        foreach(GameObject enemy in EnemiesGoArray)
        {
            Destroy(enemy);//later this should update enemies rather than destroy
        }

        GameObject newEnemy = Instantiate(EnemiesPrefabs[0], this.transform);
        newEnemy.GetComponent<EnemyDataHolder>().enemyData = enemiesData.data[0];//TODO manage array of enemies rather than using only one
        EnemiesGoArray.Add(newEnemy);


    }
}
