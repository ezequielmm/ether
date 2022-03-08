using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopBarManager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text classText;
    public TMP_Text healthText;
    public TMP_Text coinsText;


    public void SetTextValues(string nameText, string classText, string healthText, int coins)
    {
        this.nameText.text = nameText;
        this.classText.text = classText;
        this.healthText.text = healthText;
        coinsText.text = coins.ToString();
    }

}