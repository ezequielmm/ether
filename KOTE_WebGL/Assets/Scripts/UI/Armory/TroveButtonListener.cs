using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroveButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image buttonImage;
    // this is it's own class, this has to be triggered onPointerDown or the browser won't like it
    public void OnPointerDown(PointerEventData eventData)
    {
        #if !UNITY_EDITOR
        ExternalLinks.OpenLink("https://trove.treasure.lol/games/kote");
        #endif
        buttonImage.color = Color.gray;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.color = Color.white;
    }
}