using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarManager : MonoBehaviour
{
    public string currentClass;
    public int currentHealth;

    public TMP_Text nameText;
    public TMP_Text coinsText;
    public TMP_Text stageText;
    public TMP_Text healthBarText;


    [SerializeField]
    Slider healthBar;

    public GameObject classIcon, className, showmapbutton;
    public PotionsContainerManager PotionsContainerManager;

    // we need to keep track of this, as the attack response data doesn't relay this information
    private int maxHealth;
    private void Start()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);

        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener(OnToggleMapIcon);
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.AddListener(UpdateStageText);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnPlayerAttacked);

        // this has to be set here, as it is not visible in the inspector
        nameText.maxVisibleLines = 2;
        
        maxHealth = GameManager.Instance.PlayerStateData.data.playerState.hpMax;
        SetHealthText(GameManager.Instance.PlayerStateData.data.playerState.hpCurrent);
        SetCoinsText(GameManager.Instance.PlayerStateData.data.playerState.gold);
        PotionsContainerManager.OnPlayerStateUpdate(GameManager.Instance.PlayerStateData);
    } 

    private void OnToggleMapIcon(bool arg0)
    {
        Debug.Log("[OnToggleMapIcon]");
        showmapbutton.SetActive(arg0);
    }

    private void UpdateStageText(int act, int step)
    {
        stageText.SetText("STAGE " + act + "-" + (step + 1));
    }

    public void SetNameText(string nameText)
    {
        this.nameText.text = nameText;
    }


    public void SetHealthText(int health)
    {
        Debug.Log("SET HEALTH " + health);
        healthBar.value = (float)(health) / maxHealth;
        healthBarText.text = $"{health}/{maxHealth}";
    }

    public void SetCoinsText(int coins)
    {
        Debug.Log($"[TopBarManager] Coins: {coins}");
        coinsText.text = coins.ToString();
    }

    public void OnPlayerAttacked(CombatTurnData combatTurnData)
    {
        var target = combatTurnData.GetTarget("player");
        if (target == null) return;
        // only update the text if the player's health has changed
        if (target.healthDelta != 0)
        {
            Debug.Log("SetHealthText(target.finalHealth)");

            SetHealthText(target.finalHealth);
        }
    }

    public void OnPlayerStatusUpdate(PlayerStateData playerState)
    {
        Debug.Log(playerState.ToString());
        SetNameText(playerState.data.playerState.playerName);
        maxHealth = playerState.data.playerState.hpMax;
        Debug.Log("(playerState.data.playerState.hpCurrent)");
        SetHealthText(playerState.data.playerState.hpCurrent);
        SetCoinsText(playerState.data.playerState.gold);
        PotionsContainerManager.OnPlayerStateUpdate(GameManager.Instance.PlayerStateData);
    }

    public void SetClassSelected(string classSelected)
    {
        currentClass = classSelected;
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
        SetHealthText(currentHealth);
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