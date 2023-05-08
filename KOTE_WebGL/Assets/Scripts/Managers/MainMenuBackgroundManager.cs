using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBackgroundManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject parallax;
    [SerializeField] 
    private GameObject animatedElements;
        
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener(activate => SetBackgroundVisible(!activate));    
    }

    private void SetBackgroundVisible(bool visible)
    {
        parallax.SetActive(visible);
        animatedElements.SetActive(visible);
    }
}
