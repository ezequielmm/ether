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

    // we need to keep track of this, as the attack response data doesn't relay this information
    private int maxHealth;
    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(PlayerPrefs.GetString("session_token"));

        //currentClass = PlayerPrefs.GetString("class_selected");
        
        
        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.AddListener(SetProfileInfo);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusupdate);

        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener(OnToggleMapIcon);
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.AddListener(UpdateStageText);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnPlayerAttacked);

        // this has to be set here, as it is not visible in the inspector
        nameText.maxVisibleLines = 2;
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
        healthBar.value = (float)(health) / maxHealth;
        healthBarText.text = $"{health}/{maxHealth}";
    }

    public void SetCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    public void SetProfileInfo(ProfileData profileData)
    {
        SetNameText(profileData.data.name);
        SetCoinsText(profileData.data.coins);
    }

    public void OnPlayerAttacked(CombatTurnData combatTurnData) 
    {        
        var target = combatTurnData.GetTarget("player");
        if (target == null) return;
        // only update the text if the player's health has changed
        if (target.healthDelta != 0)
        {
            SetHealthText(target.finalHealth);
        }
    }
    
    public void OnPlayerStatusupdate(PlayerStateData playerState) 
    {        
        SetNameText(playerState.data.playerState.playerName);
        maxHealth = playerState.data.playerState.hpMax;
        SetHealthText(playerState.data.playerState.hpCurrent);
        SetCoinsText(playerState.data.playerState.gold);
    }

    public void SetClassSelected(string classSelected)
    {
        currentClass = classSelected;
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
        //SetHealthText(currentHealth);
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

    public void OnDeskButtonClicked()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Deck);
    }

    public void OnGearButtonClicked()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Top Bar Click");
    }
}