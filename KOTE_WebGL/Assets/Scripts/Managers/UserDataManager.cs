using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UserDataManager : SingleTon<UserDataManager>
{
    public string UserEmail => profile?.email ?? string.Empty;
    public string ExpeditionId { get; private set; } = "";
    public bool HasExpedition => expeditionStatus?.HasExpedition ?? false;
    public int ActiveNft => expeditionStatus?.NftId ?? -1;
    public List<GearItemData> EquippedGear => expeditionStatus?.EquippedGear ?? null;
    public NftContract NftContract => expeditionStatus?.TokenType ?? NftContract.None;
    public List<string> VerifiedWallets => profile?.ownedWallets ?? new();
    public ExpeditionStatus.ContestData ContestData => expeditionStatus?.Contest;
    
    ProfileData profile = null;
    ExpeditionStatus expeditionStatus = null;

    public UnityEvent ExpeditionStatusUpdated { get; } = new();

    // get the unique identifier for this instance of the client
    public string ClientId
    {
        get
        {
            string id = PlayerPrefs.GetString("client_id");
            // if the client id doesn't exist, create one and save it
            if (string.IsNullOrEmpty(id))
            {
                Guid newId = Guid.NewGuid();
                id = newId.ToString();
                PlayerPrefs.SetString("client_id", newId.ToString());
                PlayerPrefs.Save();
            }

            return id;
        }
    }

    private void Start()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnExpeditionUpdate);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(ClearDataOnLogout);
        GameManager.Instance.EVENT_SCENE_LOADED.AddListener(OnExpeditionStart);
    }

    private void ClearDataOnLogout(string _)
    {
        profile = null;
        expeditionStatus = null;
        ExpeditionId = null;
    }
    public async UniTask UpdatePlayerProfile() 
    {
        profile = await FetchData.Instance.GetPlayerProfile();
        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.Invoke(profile?.name ?? string.Empty, profile?.fief ?? -1);
    }

    public async UniTask UpdateExpeditionStatus() 
    {
        SetExpedition(await FetchData.Instance.GetExpeditionStatus());
    }

    public async UniTask ClearExpedition()
    {
        SetExpedition(null);
        await SendData.Instance.ClearExpedition();
    }

    public void OnExpeditionStart(inGameScenes sceneType)
    {
        if (sceneType != inGameScenes.Expedition) return;
        UpdateExpeditionStatus();
    }

    public void SetExpedition(ExpeditionStatus newStatus)
    {
        expeditionStatus = newStatus;
        ExpeditionStatusUpdated.Invoke();
    }

    private void OnExpeditionUpdate(PlayerStateData playerState)
    {
        ExpeditionId = playerState.data.expeditionId;
    }
}
