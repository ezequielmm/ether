using System.Collections;
using UnityEngine;

public class BattleBottomUIManager : MonoBehaviour
{
    [SerializeField] private GameObject container;

    private void Start()
    {
        container.SetActive(false);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE
            .AddListener(OnNodeDataUpdated); //TODO: this will change as not all nodes will be combat
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.AddListener(OnCombatElementsToggle);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_UI.AddListener(OnToggleCombatUi);
    }

    private void OnCombatElementsToggle(bool toggleStatus)
    {
        //we need to hide or content when the map panel is on
        if (toggleStatus)
        {
            container.SetActive(true);
            return;
        }

        GameManager.Instance.EVENT_FADE_OUT_UI.Invoke();
        StartCoroutine(WaitForFade());
    }

    private IEnumerator WaitForFade()
    {
        yield return new WaitForSeconds(GameSettings.UI_FADEOUT_TIME + 0.1f);
        container.SetActive(false);
    }

    private void OnToggleCombatUi(bool activate)
    {
        container.SetActive(activate);
    }

    private void OnNodeDataUpdated(NodeStateData nodeData, WS_QUERY_TYPE wsType)
    {
        if (wsType == WS_QUERY_TYPE.MAP_NODE_SELECTED)
        {
            container.SetActive(true);
        }
    }
}