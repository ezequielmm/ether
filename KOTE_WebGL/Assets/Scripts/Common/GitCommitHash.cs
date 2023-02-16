using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GitCommitHash
{
    public static string CommitHash 
    {
        get 
        {
            GitHash hashObj = Resources.Load<GitHash>("GitCommitHash");
            return hashObj?.CommitHash;
        }
    }
}
