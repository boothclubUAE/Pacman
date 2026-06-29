namespace UnityToolBox.BuildTools
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public partial class BuildToolsEditorWindow
    {
        [SerializeField] private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        [SerializeField] private BuildType buildType = BuildType.Development;
        [SerializeField] private string developmentFlags = string.Empty;
        public string buildLocation = "";
        public string productName = "";
        [SerializeField] private string productionFlags = string.Empty;
        [SerializeField] private bool shouldAutoRun = false;
        private string selectedFlags = string.Empty;

        private enum BuildType
        {
            Development,
            Production
        }

        private void LoadBuildOptions()
        {
            buildTarget = (BuildTarget)EditorPrefs.GetInt("_buildTarget", (int)buildTarget);
            buildType = (BuildType)EditorPrefs.GetInt("_buildType", (int)buildType);
            developmentFlags = EditorPrefs.GetString("_developmentFlags", developmentFlags);
            buildLocation = EditorPrefs.GetString("_buildLocation", buildLocation);
            productName = EditorPrefs.GetString("_productName", productName);
            productionFlags = EditorPrefs.GetString("_productionFlags", productionFlags);
            shouldAutoRun = EditorPrefs.GetBool("_autoRun", shouldAutoRun);
        }

        private void ListBuildUtility()
        {
            EditorGUI.BeginChangeCheck();

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt("_buildTarget", (int)buildTarget);
                LoadTargetSpecificOptions();
            }

            EditorGUI.BeginChangeCheck();

            buildType = (BuildType)EditorGUILayout.EnumPopup("Build Type", buildType);
            if (buildType == BuildType.Development)
            {
                developmentFlags = EditorGUILayout.TextField("Development Flags", developmentFlags);
                selectedFlags = developmentFlags;
            }
            else
            {
                productionFlags = EditorGUILayout.TextField("Production Flags", productionFlags);
                selectedFlags = productionFlags;
            }

            productName = EditorGUILayout.TextField("Product Name", productName);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(buildLocation, EditorStyles.centeredGreyMiniLabel);


            if (GUILayout.Button("Central Build Location"))
            {

            }


            buildLocation = EditorGUILayout.TextField("Build Location", buildLocation);

            EditorGUILayout.Space(10);

            shouldAutoRun = EditorGUILayout.Toggle("Run On Build", shouldAutoRun);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt("_buildType", (int)buildType);
                EditorPrefs.SetString("_developmentFlags", developmentFlags);
                EditorPrefs.SetString("_productionFlags", productionFlags);
                EditorPrefs.SetString("_buildLocation", buildLocation);
                EditorPrefs.SetString("_productName", productName);
                EditorPrefs.SetBool("_autoRun", shouldAutoRun);
            }
        }
    }
}
