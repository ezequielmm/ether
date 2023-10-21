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

        switch (selectedOption)
        {
            case Options.Dev:
                settings.activeProfileId = "5a961f23dd1267943b6be1009ecadc22";
                BuildWebGLProject();
                Debug.Log("Has seleccionado la Opci�n 1.");
                // Realiza acciones relacionadas con la Opci�n 1 aqu�.
                break;
            case Options.Test:
                settings.activeProfileId = "a238f648cfdfd90489a7fe34fc776771";
                BuildWebGLProject();
                Debug.Log("Has seleccionado la Opci�n 2.");
                // Realiza acciones relacionadas con la Opci�n 2 aqu�.
                break;
            case Options.Test2:
                settings.activeProfileId = "9dd45e67f571dfe4cbfa918393eb58d4";
                BuildWebGLProject();
                Debug.Log("Has seleccionado la Opci�n 2.");
                // Realiza acciones relacionadas con la Opci�n 2 aqu�.
                break;
            case Options.Demo:
                settings.activeProfileId = "21214cbebb05b2b4b9dfacb170efeb96";
                BuildWebGLProject();
                Debug.Log("Has seleccionado la Opci�n 3.");
                // Realiza acciones relacionadas con la Opci�n 3 aqu�.
                break;
            default:
                Debug.LogError("Opci�n no reconocida.");
                break;
        }
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

        Debug.Log("El proyecto ha sido compilado para WebGL");
    }
}