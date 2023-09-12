using System.Collections.Generic;
using UnityEngine;

public class CombatBackground : MonoBehaviour
{
    [SerializeField] private GameObject _defaultBackground;
    [SerializeField] private Transform _pivot;
    
    [SerializeField] private List<ActBackgroundsList> _combatBackgroundList;

    private GameObject _currentBackgroundPrefab;
    private GameObject _currentBackground;
    
    private void Awake()
    {
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.AddListener(SetBackground);
    }

    private void SetBackground(int act, int step, bool isBoss)
    {
        var bg = TakeBackground(act, step, isBoss);

        if (bg == _currentBackgroundPrefab) return;

        if (_currentBackground != null)
            Destroy(_currentBackground);
        _currentBackground = Instantiate(bg, Vector3.zero, Quaternion.identity, _pivot);
        _currentBackgroundPrefab = bg;
    }

    private GameObject TakeBackground(int act, int step, bool isBoss)
    {
        var actBackgrounds = _combatBackgroundList[act];

        if (isBoss && actBackgrounds.BossBackground != null)
            return actBackgrounds.BossBackground;
            
        
        for (var i = actBackgrounds.BackgroundsList.Count - 1; i >= 0; i--)
        {
            // if the step is less than the step for the background, continue
            if (step < actBackgrounds.BackgroundsList[i].Step)
                continue;

            // else set that as the background and continue
            return actBackgrounds.BackgroundsList[i].Background;
        }

        Debug.LogError($"No background found for act {act} step {step}");
        return _defaultBackground;
    }
}