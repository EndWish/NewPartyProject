using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class EditorStartInit
{
    [MenuItem("MyEditor/���� ������ ����")]
    public static void SetupFromStartScene() {
        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
        EditorSceneManager.playModeStartScene = sceneAsset;
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [MenuItem("MyEditor/���� ������ ����")]
    public static void StartFromThisScene() {
        EditorSceneManager.playModeStartScene = null;
        UnityEditor.EditorApplication.isPlaying = true;
    }
}