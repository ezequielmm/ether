using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SendCardsSelected(List<string> cardIds)
    {
        CardsSelectedList cardList = new CardsSelectedList { cardsToTake = cardIds };
        socketRequest.SendData(SocketEvent.MoveSelectedCard, cardList);
    }
}
