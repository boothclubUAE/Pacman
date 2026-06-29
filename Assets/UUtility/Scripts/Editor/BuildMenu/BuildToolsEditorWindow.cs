using UnityEditor;
using UnityEngine;

namespace UnityToolBox.BuildTools
{
    public partial class BuildToolsEditorWindow : EditorWindow
    {
        [MenuItem("OderaBuildTools/Open OderaBuildTools")]
        static private void OpenBuildTools()
        {
            GetWindow(typeof(BuildToolsEditorWindow), false, "OderaBuildTools", true);
        }

        private void OnEnable()
        {
            LoadBuildOptions();
            LoadTargetSpecificOptions();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Clean Utility", EditorStyles.boldLabel);
            ListCleanUtility();
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            //EditorGUILayout.LabelField("Prepare Utility", EditorStyles.boldLabel);
            //ListPrepareUtility();
            //EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("Shared Build Options", EditorStyles.boldLabel);
            ListBuildUtility();
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("Target Specific Build Options", EditorStyles.boldLabel);
            ListTargetSpecificOptions();
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            ListBuildCommand();
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Odera Build Tools 1.0", EditorStyles.centeredGreyMiniLabel);
        }
    }
}
