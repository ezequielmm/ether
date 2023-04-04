using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject combatContainer;
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemyManager;
    [SerializeField] private GameObject pointer;

    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.AddListener(ShowCombatElements);
        GameManager.Instance.EVENT_SHOW_PLAYER_CHARACTER.AddListener(OnShowPlayerCharacter);
    }

    private void ShowCombatElements(bool data)
    {
        if (data == false)
        {
            pointer.SetActive(false);
            return;
        }
        enemyManager.SetActive(true);
        pointer.SetActive(true);
        hand.SetActive(true);
        combatContainer.SetActive(true);
        player.SetActive(true);
    }

    // we need a way of only turning on the player for non-combat nodes
    private void OnShowPlayerCharacter()
    {
        combatContainer.SetActive(true);
        hand.SetActive(false);
        player.SetActive(true);
        enemyManager.SetActive(false);
        pointer.SetActive(false);
    }
}