using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    public void LoadCombat()
    {
        GameManager.Instance.LoadScene(inGameScenes.Combat);
    }

    public void ShowShop()
    {

    }
}
