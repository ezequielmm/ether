using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelManager : MonoBehaviour
{
    public GameObject mainMenu;

    [SerializeField]
    Text errorMessageText;

    [SerializeField]
    InputField emailInputField;

    [SerializeField]
    InputField passwordInputField;

    [SerializeField]
    Toggle rememberMeToggle;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Login()
    {
        mainMenu.SetActive(true);
        Destroy(this.gameObject);
    }
    public void ClosePanel()
    {
        mainMenu.SetActive(true);
        Destroy(this.gameObject);
    }
}
