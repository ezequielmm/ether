using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ClientEnvironmentManager;

[CreateAssetMenu(fileName = "UnityEnvironment", menuName = "ScriptableObjects/UnityEnvironment")]
public class UnityEnvironment : ScriptableObject
{
    public Environments EnvironmentToEmulate = Environments.Dev;
}
