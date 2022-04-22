using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBottomUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

    private void Start()
    {
        container.SetActive(false);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);//TODO: this will change as not all nodes will be combat
    }

    private void OnNodeDataUpdated(NodeStateData arg0)
    {
        container.SetActive(true);
    }
}
