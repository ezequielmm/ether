using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusIcon : MonoBehaviour
{

    [System.Serializable]
    private class IconMap
    {
        public ENEMY_INTENT type;
        public int valueThreshold;
        public Sprite icon;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
