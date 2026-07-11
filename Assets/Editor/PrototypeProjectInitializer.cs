#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MmorpgPrototype.Editor
{
    [InitializeOnLoad]
    public static class PrototypeProjectInitializer
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";
        private const string OpenedKey = "MmorpgPrototype.OpenedPrototypeScene";

        static PrototypeProjectInitializer()
        {
            EditorApplication.delayCall += EnsurePrototypeScene;
        }

        public static void EnsurePrototypeScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Directory.CreateDirectory("Assets/Scenes");

            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            if (!SessionState.GetBool(OpenedKey, false) && SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
                SessionState.SetBool(OpenedKey, true);
            }
        }
    }
}
#endif
