using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendData : DataManager, ISingleton<SendData>
{
    private static SendData instance;
    public static SendData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SendData();
            }
            return instance;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }

    public async UniTask SendCardsSelected(List<string> cardIds)
    {
        CardsSelectedList cardList = new CardsSelectedList { cardsToTake = cardIds };
        await socketRequest.SendData(SocketEvent.MoveSelectedCard, cardList);
    }

    public async UniTask ClearExpedition() 
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionCancel);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.AddAuthToken();
            await webRequest.MakeRequest(request);
        }
    }
}
