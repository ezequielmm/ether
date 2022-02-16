using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegistryPanelManager : MonoBehaviour
{
    public GameObject mainMenu;

    [SerializeField]
    Text errorMessageText;

    [SerializeField]
    InputField usernameInputField;

    [SerializeField]
    InputField emailInputField;
    [SerializeField]
    InputField emailConfirmInputField;

    [SerializeField]
    InputField passwordInputField;
    [SerializeField]
    InputField passwordConfirmInputField;

    [SerializeField]
    Toggle acceptTermsAndConditionsToggle;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Register()
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
