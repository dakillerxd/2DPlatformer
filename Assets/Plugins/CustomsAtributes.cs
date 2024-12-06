using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

namespace CustomAttributes
{
    #region ReadOnly Attribute
    /// <summary>
    /// Makes a field read-only in the inspector
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var previousColor = GUI.color;
            var previousEnabled = GUI.enabled;

            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, 0.8f);
            GUI.enabled = false;

            EditorGUI.PropertyField(position, property, label, true);

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }
    }
    #endif
    #endregion

    // #region CustomButton Attribute
    // /// <summary>
    // /// Adds a custom button to the inspector
    // /// </summary>
    // [System.AttributeUsage(System.AttributeTargets.Method)]
    // public class CustomButtonAttribute : System.Attribute
    // {
    //     public string Name { get; private set; }
    //     public string Tooltip { get; private set; }
    //     public int Height { get; private set; }
    //     public int Space { get; private set; }
    //     public string Color { get; private set; }
    //     public bool RecordUndo { get; private set; }
    //     public string UndoMessage { get; private set; }
    //
    //     public CustomButtonAttribute(
    //         string name = null,
    //         string tooltip = null,
    //         int height = 30,
    //         int space = 0,
    //         string color = "White",
    //         bool recordUndo = true,
    //         string undoMessage = null)
    //     {
    //         if (string.IsNullOrEmpty(name))
    //         {
    //             Name = null;
    //         }
    //         else
    //         {
    //             Name = name;
    //         }
    //         Tooltip = tooltip;
    //         Height = height;
    //         Space = space;
    //         Color = color;
    //         RecordUndo = recordUndo;
    //         UndoMessage = undoMessage ?? $"CustomButton Action: {name ?? "Unnamed"}";
    //     }
    // }
    //
    // #if UNITY_EDITOR
    // [CustomEditor(typeof(MonoBehaviour), true, isFallback = false)]
    // public class CustomMonoBehaviourEditor : Editor
    // {
    //     private bool hasCustomAttributes;
    //     
    //     private void OnEnable()
    //     {
    //         var targetType = target.GetType();
    //         
    //         var hasButtonMethods = targetType
    //             .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    //             .Any(m => m.GetCustomAttributes(typeof(CustomButtonAttribute), false).Length > 0);
    //
    //         var hasReadOnlyFields = targetType
    //             .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    //             .Any(f => f.GetCustomAttributes(typeof(ReadOnlyAttribute), false).Length > 0);
    //
    //         hasCustomAttributes = hasButtonMethods || hasReadOnlyFields;
    //     }
    //
    //     private static readonly Dictionary<string, Color> colorMap = new Dictionary<string, Color>
    //     {
    //         {"Grey", Color.grey},
    //         {"Red", Color.red},
    //         {"Green", Color.green},
    //         {"Blue", Color.blue},
    //         {"Yellow", Color.yellow},
    //         {"Cyan", Color.cyan},
    //         {"Magenta", Color.magenta},
    //         {"White", Color.white},
    //         {"Black", Color.black}
    //     };
    //
    //     private string FormatMethodName(string methodName)
    //     {
    //         return Regex.Replace(methodName, "(?<!^)(?=[A-Z])", " ");
    //     }
    //
    //     private Color ParseColor(string colorName)
    //     {
    //         if (string.IsNullOrEmpty(colorName)) return GUI.color;
    //
    //         colorName = colorName.Trim();
    //         if (colorMap.TryGetValue(colorName, out Color color))
    //         {
    //             return color;
    //         }
    //
    //         if (colorName.StartsWith("#") && ColorUtility.TryParseHtmlString(colorName, out Color hexColor))
    //         {
    //             return hexColor;
    //         }
    //
    //         return GUI.color;
    //     }
    //
    //     public override void OnInspectorGUI()
    //     {
    //         if (!hasCustomAttributes)
    //         {
    //             base.OnInspectorGUI();
    //             return;
    //         }
    //
    //         DrawDefaultInspector();
    //         DrawButtons();
    //     }
    //
    //     private void DrawButtons()
    //     {
    //         var mono = target as MonoBehaviour;
    //         var methods = mono.GetType()
    //             .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    //             .Where(m => m.GetCustomAttributes(typeof(CustomButtonAttribute), false).Length > 0);
    //
    //         foreach (var method in methods)
    //         {
    //             var buttonAttr = method.GetCustomAttribute<CustomButtonAttribute>();
    //             var buttonName = string.IsNullOrEmpty(buttonAttr.Name) ? FormatMethodName(method.Name) : buttonAttr.Name;
    //             var buttonContent = new GUIContent(buttonName, buttonAttr.Tooltip);
    //
    //             if (buttonAttr.Space > 0)
    //             {
    //                 GUILayout.Space(buttonAttr.Space);
    //             }
    //
    //             var originalColor = GUI.color;
    //             GUI.color = ParseColor(buttonAttr.Color);
    //
    //             var options = new GUILayoutOption[] { 
    //                 GUILayout.Height(buttonAttr.Height)
    //             };
    //
    //             if (GUILayout.Button(buttonContent, options))
    //             {
    //                 if (buttonAttr.RecordUndo)
    //                 {
    //                     Undo.RecordObject(target, buttonAttr.UndoMessage);
    //                 }
    //
    //                 var parameters = method.GetParameters();
    //                 if (parameters.Length == 0)
    //                 {
    //                     method.Invoke(mono, null);
    //
    //                     if (buttonAttr.RecordUndo)
    //                     {
    //                         EditorUtility.SetDirty(target);
    //                     }
    //                 }
    //                 else
    //                 {
    //                     Debug.LogWarning($"Method {method.Name} has parameters and cannot be used with CustomButtonAttribute");
    //                 }
    //             }
    //
    //             GUI.color = originalColor;
    //         }
    //     }
    // }
    // #endif
    // #endregion

    #region Sorting Layer Field
    [System.Serializable]
    public class SortingLayerField
    {
        [SerializeField]
        private string m_LayerName = "Default";
        [SerializeField]
        private int m_LayerID;

        public int LayerID => m_LayerID;
        public string LayerName => m_LayerName;

        public static implicit operator int(SortingLayerField sortingField)
        {
            return sortingField.m_LayerID;
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SortingLayerField))]
    public class SortingLayerFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            
            SerializedProperty layerName = property.FindPropertyRelative("m_LayerName");
            SerializedProperty layerID = property.FindPropertyRelative("m_LayerID");
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect popupRect = new Rect(position.x, position.y, position.width - 30, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 25, position.y, 25, position.height);

            string[] sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            int[] layerIDs = SortingLayer.layers.Select(l => l.id).ToArray();
            
            int currentIndex = System.Array.IndexOf(sortingLayerNames, layerName.stringValue);
            if (currentIndex == -1) currentIndex = 0;
            
            int newIndex = EditorGUI.Popup(popupRect, currentIndex, sortingLayerNames);
            if (newIndex != currentIndex)
            {
                layerName.stringValue = sortingLayerNames[newIndex];
                layerID.intValue = layerIDs[newIndex];
            }

            if (GUI.Button(buttonRect, "..."))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
                var settingsWindow = EditorWindow.GetWindow<EditorWindow>("Project Settings");
                var tagsAndLayersHeader = typeof(Editor).Assembly.GetType("UnityEditor.TagManager");
                var window = SettingsService.OpenProjectSettings("Project/Tags and Layers");
            }
            
            EditorGUI.EndProperty();
        }
    }
    #endif
    #endregion

    #region Scene Field
    [System.Serializable]
    public class SceneField
    {
        [SerializeField]
        private Object m_SceneAsset;
        [SerializeField]
        private string m_SceneName = "";
        [SerializeField]
        private int m_BuildIndex = -1;

        public string SceneName => m_SceneName;
        public int BuildIndex => m_BuildIndex;

        public static implicit operator string(SceneField sceneField) => sceneField.SceneName;
        public static implicit operator int(SceneField sceneField) => sceneField.BuildIndex;
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
                        
                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        buildIndex.intValue = -1;

                        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
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
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
    #endif
    #endregion
}