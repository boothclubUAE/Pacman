namespace UnityToolBox.BuildTools
{
    using UnityEditor;
    using UnityEngine;

    public partial class BuildToolsEditorWindow
    {
        private void ListCleanUtility()
        {
            if (GUILayout.Button("Clear All Player Prefs"))
            {
                CleanPlayerPrefs();
            }

            //if (GUILayout.Button("Clear Build Folder"))
            //{
            //    CleanBuildFolder();
            //}
        }

        private void CleanPlayerPrefs()
        {
            Debug.Log(BuildToolsUtils.LogLabel + "Cleared Player Prefs");
            PlayerPrefs.DeleteAll();
        }

        private void CleanBuildFolder()
        {
            Debug.Log(BuildToolsUtils.LogLabel + "Cleared Build Folder");
            FileUtil.DeleteFileOrDirectory(BuildToolsUtils.BuildPathRoot);
        }
    }
}
