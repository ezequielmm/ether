using Spine;
using Spine.Unity;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    public GameObject pointerContainer;
    public GameObject pointerTarget;
    public SkeletonAnimation skeletonAnimation;
    [SpineBone(dataField: "skeletonAnimation")]
    public string boneName;

    private Bone aimBone;
    [HideInInspector] public bool overEnemy;

    private void Start()
    {
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
        aimBone = skeletonAnimation.Skeleton.FindBone(boneName);
    }


    private void OnPointerActivated()
    {
        pointerContainer.SetActive(true);
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        // I don't know why this is so weird, the x and y axis needs to be flipped, and there has to be an offset
        aimBone.SetLocalPosition(skeletonAnimation.transform.InverseTransformPoint(new Vector3(position.y + 2, -position.x - 2, 0)));
        pointerTarget.transform.position = position;
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