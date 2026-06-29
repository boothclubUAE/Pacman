namespace UnityToolBox.BuildTools
{
    using UnityEditor;

    public partial class BuildToolsEditorWindow
    {
        private void LoadTargetSpecificOptions()
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    LoadAndroidSpecificOptions();
                    break;

                default:
                    break;
            }
        }

        private void ListTargetSpecificOptions()
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    ListAndroidSpecificOptions();
                    break;

                default:
                    EditorGUILayout.LabelField("No specific Options for this Target");
                    break;
            }
        }

        private void ExecutePlatformSpecificPreprocess()
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    ExecuteWindowsSpecificPreProcess();
                    break;

                case BuildTarget.Android:
                    ExecuteAndroidSpecificPreProcess();
                    break;

                default:
                    break;
            }
        }

        private void ExecutePlatformSpecificPostProcess()
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    ExecuteAndroidSpecificPostProcess();
                    break;

                default:
                    break;
            }
        }
    }
}
