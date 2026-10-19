using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class MinigameDefinition
    {
        public string SceneName;
        public string TitleText;
        [TextArea(2, 5)] public string TutorialText;
        public Sprite Icon;
        public Sprite Image;
    }

    [CreateAssetMenu(fileName = "MinigamesDefinitionsSO", menuName = "ScriptableObjects/MinigamesDefinitionsSO")]
    public class MinigamesDefinitionsSO : ScriptableObject
    {
        public List<MinigameDefinition> Minigames = new List<MinigameDefinition>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var minigame in Minigames)
            {
                if (string.IsNullOrEmpty(minigame.SceneName))
                    continue;

                if (!IsInBuildSettings(minigame.SceneName))
                    Debug.LogWarning($"[MinigamesDefinitionsSO] '{minigame.SceneName}' is not in Build Settings.", this);
            }
        }

        private static bool IsInBuildSettings(string sceneName)
        {
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName)
                    return true;
            }
            return false;
        }
#endif
    }
}
