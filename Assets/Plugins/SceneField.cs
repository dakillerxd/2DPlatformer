using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomAttribute
{
    [System.Serializable]
    public class SceneField
    {
        [SerializeField]
        private Object m_SceneAsset;
        [SerializeField]
        private string m_SceneName = "";
        [SerializeField]
        private int m_BuildIndex = -1;

        public string SceneName
        {
            get { return m_SceneName; }
        }

        public int BuildIndex
        {
            get { return m_BuildIndex; }
        }

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }

        public static implicit operator int(SceneField sceneField)
        {
            return sceneField.BuildIndex;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            SerializedProperty sceneAsset = property.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneName = property.FindPropertyRelative("m_SceneName");
            SerializedProperty buildIndex = property.FindPropertyRelative("m_BuildIndex");
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null)
            {
                EditorGUI.BeginChangeCheck();
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (sceneAsset.objectReferenceValue != null)
                    {
                        SceneAsset scene = sceneAsset.objectReferenceValue as SceneAsset;
                        sceneName.stringValue = scene.name;
                        
                        // Find build index
                        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        buildIndex.intValue = -1;
                        
                        for (int i = 0; i < scenes.Length; i++)
                        {
                            if (scenes[i].path == scenePath)
                            {
                                buildIndex.intValue = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        sceneName.stringValue = "";
                        buildIndex.intValue = -1;
                        
                        Debug.LogError("Scene is not in build index");
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}