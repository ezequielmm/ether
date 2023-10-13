using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

public class OptionSelector : EditorWindow
{
    private enum Options
    {
        Dev,
        Test,
        Test2,
        Demo
    }

    private string versionNumber;

    private Options selectedOption = Options.Dev;

    [MenuItem("Build Menu/Select Environment")]
    public static void ShowWindow()
    {
        OptionSelector window = GetWindow<OptionSelector>("Version Number Editor");
        window.LoadVersionNumber();

        GetWindow<OptionSelector>("Select Environment");
    }

    private void OnGUI()
    {

        GUILayout.Label("Player Settings Version Number", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        versionNumber = EditorGUILayout.TextField("Version Number", versionNumber);

        if (EditorGUI.EndChangeCheck())
        {
            PlayerSettings.bundleVersion = versionNumber;
        }

        if (GUILayout.Button("Update Player Settings"))
        {
            PlayerSettings.bundleVersion = versionNumber;
            Debug.Log("Player Settings version number updated to: " + versionNumber);
        }

        GUILayout.Label("Selecciona una opci�n:", EditorStyles.boldLabel);

        selectedOption = (Options)EditorGUILayout.EnumPopup("Opciones:", selectedOption);

        if (GUILayout.Button("Aceptar"))
        {
            HandleSelectedOption();
        }
    }

    private void HandleSelectedOption()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        //AddressableAssetProfileSettings profile = settings.profileSettings;
        //string profileID = settings.profileSettings.GetProfileId(profileName);

        var profileId = selectedOption switch
        {
            Options.Dev => "5a961f23dd1267943b6be1009ecadc22",
            Options.Test => "a238f648cfdfd90489a7fe34fc776771",
            Options.Test2 => "ac8c9f15be2660a4788e80f44c3b3ad6",
            Options.Demo => "21214cbebb05b2b4b9dfacb170efeb96",
            _ => ""
        };
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogError("Opci�n no reconocida.");
            return;
        }
        
        settings.activeProfileId = profileId;
        BuildWebGLProject();
    }

    void LoadVersionNumber()
    {
        versionNumber = PlayerSettings.bundleVersion;
    }

    private void BuildWebGLProject()
    {
        // Configurar las opciones de compilaci�n para WebGL.
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.targetGroup = BuildTargetGroup.WebGL;
        buildPlayerOptions.locationPathName = "Builds/WebGL"; // Ruta de salida de la compilaci�n.

        // Realizar la compilaci�n.
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        Debug.Log("El proyecto ha sido compilado para WebGL.");
    }
}
