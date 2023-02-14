using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionLabelManager : MonoBehaviour
{
    public TMP_Text versionText;
    public TMP_Text clientIdText;
    
    // Start is called before the first frame update
    void Start()
    {
        versionText.text = "V " + Application.version;
        clientIdText.text = GameManager.ClientId;
    }
}
