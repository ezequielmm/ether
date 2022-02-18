using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTemporary : MonoBehaviour
{
    public void LoadMap() => GameManager.Instance.LoadScene(inGameScenes.Map);
}