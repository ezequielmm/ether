using Spine.Unity;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    public GameObject pointerContainer;
    public SkeletonAnimation SkeletonAnimation;

    public bool overEnemy;

    private void Start()
    {
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
    }

   

    private void OnPointerActivated()
    {
        pointerContainer.SetActive(true);
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        SkeletonAnimation.transform.position = position;
    }


    private void OnPointerDeactivated(string id)
    {
        pointerContainer.SetActive(false);
        //if the pointer is over an enemy, play the card
        if (overEnemy)
        {
            GameManager.Instance.EVENT_CARD_PLAYED.Invoke(id);
        }

        // else return it to the deck
        //GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(id);
    }
}