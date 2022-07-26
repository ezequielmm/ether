using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [SerializeField]
    GameObject iconPrefab;
    [SerializeField]
    SpriteSpacer iconContainer;
    
    
    void Start()
    {
        if (iconPrefab == null)
        {
            Debug.LogError($"[StatusManager] Missing Icon Prefab.");
        }
        if (iconContainer == null)
        {
            Debug.LogError($"[StatusManager] Missing Icon Container.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
