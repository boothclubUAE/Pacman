using System.IO;
using UnityEngine;
using UnityEditor;

using UTool;
using UTool.Utility;

namespace UTool.Editor
{
    public class UTContextMenu : MonoBehaviour
    {
        private const string UTPath = "Assets/UUtility/UTool.prefab";

        private const string UToolEditorASMREF = "UTool.Editor.asmref";
        private static string UToolEditorASMREFPath => $"Assets/UUtility/Scripts/Editor/AsmRef/{UToolEditorASMREF}";

        private static string screenshotFolder => Application.dataPath.Replace("/Assets", "") + "/Screenshots";

        [MenuItem("UT/Screenshot", false, 50)]
        public static void Screenshot()
        {
            UUtility.CheckAndCreateDirectory(screenshotFolder);
            string[] files = Directory.GetFiles(screenshotFolder);

            string index = files.Length > 0 ? "(" + files.Length + ")" : "";

            ScreenCapture.CaptureScreenshot(screenshotFolder + $"/screenshot{index}.png");
        }

        [MenuItem("UT/Folder/ScreenshotFolder", false, 0)]
        public static void OpenScreenshotFolder()
        {
            System.Diagnostics.Process.Start(screenshotFolder);
        }

        [MenuItem("GameObject/UT/UTool", false, 0)]
        private static void CreateUTMPText(MenuCommand command)
        {
            GameObject utObject = UTContextMenuUtility.CreateAssetFromPath(command, UTPath, prefabLink: true, select: false);
            UT ut = utObject.GetComponent<UT>();

            ut.SetProjectName(Application.productName);
        }

        [MenuItem("Assets/UT/Create UTool.Editor Assembly Reference", false, 9)]
        private static void CreateAssemblyReference(MenuCommand command)
        {
            if (Selection.assetGUIDs.Length == 0)
                return;

            string currentPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if (AssetDatabase.CopyAsset(UToolEditorASMREFPath, $"{currentPath}/{UToolEditorASMREF}"))
                Debug.LogWarning("UT Editor : Failed to Create UTool.Editor Assembly Reference");
        }
    }
}