using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpineAnimationsManagement))]
public class SpineAnimationsManagementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpineAnimationsManagement spineAnimationsManagement = (SpineAnimationsManagement) target;

        if (GUILayout.Button("Update properties"))
        {
            spineAnimationsManagement.SetProperties();
        }
    }
}