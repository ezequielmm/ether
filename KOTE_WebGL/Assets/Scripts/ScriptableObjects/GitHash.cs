using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GitHash", menuName = "ScriptableObjects/GitHash")]
public class GitHash : ScriptableObject
{
    public string CommitHash;
}