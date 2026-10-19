using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public class SceneController
    {
        private string _currentScene;

        public bool HasLoadedScene => _currentScene != null;

        public IEnumerator LoadMinigame(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError($"[SceneController] '{sceneName}' could not be loaded");
                yield break;
            }

            yield return operation;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            _currentScene = sceneName;
        }

        public IEnumerator UnloadCurrent()
        {
            if (_currentScene == null)
                yield break;

            yield return SceneManager.UnloadSceneAsync(_currentScene);
            _currentScene = null;

            yield return Resources.UnloadUnusedAssets();
        }
    }
}
