using UnityEngine;

public class Dragon : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;

    private void Start()
    {
        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        spineAnimationsManagement.PlayAnimationSequence("Flying");
    }
}