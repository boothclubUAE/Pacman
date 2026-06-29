namespace UnityToolBox.BuildTools
{
    using UnityEditor;
    using UnityEngine;

    public partial class BuildToolsEditorWindow
    {
        private string buildExtension = string.Empty;

        private void ListBuildCommand()
        {
            if (GUILayout.Button("Build"))
            {
                try
                {
                    PreProcessBuild();
                    ExecutePlatformSpecificPreprocess();
                    ExecuteBuild();
                }
                finally 
                {
                    PostProcessBuild();
                    ExecutePlatformSpecificPostProcess();
                }
            }
        }

        private void PreProcessBuild()
        {
            SetScriptingSymbols(selectedFlags);
        }

        private void ExecuteBuild()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = string.Format("{0}/{1}/{2}/{3}{4}",
                                   buildLocation,
                                   buildType.ToString(),
                                   buildTarget.ToString(),
                                   productName,
                                   buildExtension),
                target = buildTarget,
                options = shouldAutoRun ? BuildOptions.AutoRunPlayer : BuildOptions.None,
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private void PostProcessBuild()
        {
            buildExtension = string.Empty;
            SetScriptingSymbols(string.Empty);
        }

        private void SetScriptingSymbols(string symbols)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
        }
    }
}
