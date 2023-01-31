using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommitLabel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text hashText;
    // Start is called before the first frame update
    void Start()
    {
        if (hashText == null) 
        {
            hashText = GetComponent<TMP_Text>();
        }
        hashText.text = CommitHash.Substring(0,7);
    }

    public static string CommitHash 
    {
        get 
        {
            GitHash hashObj = Resources.Load<GitHash>("GitCommitHash");
            return hashObj?.CommitHash ?? "404";
        }
    }
}
