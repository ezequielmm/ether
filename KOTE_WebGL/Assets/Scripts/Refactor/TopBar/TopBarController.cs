using UnityEngine;

namespace Refactor.TopBar
{
    public class TopBarController : MonoBehaviour
    {
        [SerializeField] private TopBarUI topBarUI;

        private void Start()
        {
            topBarUI.OnDrinkPotion += DrinkPotion;
            topBarUI.OnDiscardPotion += DiscardPotion;
            
            topBarUI.Init(GameManager.Instance.PlayerStateData);
            topBarUI.UpdatePlayerState(GameManager.Instance.PlayerStateData);
            
            GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);

            GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener(OnToggleMapIcon);
            GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnPlayerAttacked);
            GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.AddListener(UpdateStageText);
        }

        private void DiscardPotion(string potionId)
        {
            GameManager.Instance.EVENT_POTION_DISCARDED.Invoke(potionId);
        }

        private void DrinkPotion(string potionid, string targetid)
        {
            GameManager.Instance.EVENT_POTION_USED.Invoke(potionid, targetid);
        }

        private void OnToggleMapIcon(bool arg0)
        {
            topBarUI.ToogleMapIcon(arg0);
        }

        private void UpdateStageText(int act, int step)
        {
            topBarUI.UpdateStageText(act, step);
        }

        public void OnPlayerAttacked(CombatTurnData combatTurnData)
        {
            var target = combatTurnData.GetTarget("player");
            if (target == null) return;
            // only update the text if the player's health has changed
            if (target.healthDelta != 0)
            {
                Debug.Log("SetHealthText(target.finalHealth)");

                topBarUI.SetHealthText(target.finalHealth);
            }
        }

        public void OnPlayerStatusUpdate(PlayerStateData playerState)
        {
            topBarUI.UpdatePlayerState(playerState);
        }

        public void OnMapButtonClicked()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
            GameManager.Instance.EVENT_MAP_ICON_CLICKED.Invoke();
        }

        public void OnSettingsButton()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
            GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
        }

        public void OnDeckButtonClicked()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
            GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Deck);
        }

        public void OnGearButtonClicked()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
        }
    }
}