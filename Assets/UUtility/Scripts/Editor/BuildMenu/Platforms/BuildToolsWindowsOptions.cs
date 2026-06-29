namespace UnityToolBox.BuildTools
{
    public partial class BuildToolsEditorWindow
    {
        private const string WindowsExtension = ".exe";

        private void ExecuteWindowsSpecificPreProcess()
        {
            buildExtension = WindowsExtension;
        }
    }
}
