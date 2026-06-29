using UnityEngine;
using UnityEditor;

namespace UTool.Editor
{
    public class UTMPContextMenu : MonoBehaviour
    {
        private const string UTMPTextPath = "Assets/UUtility/Prefabs/UTMP/Text (UTMP)/Text (UTMP).prefab";
        private const string UTMPInputField = "Assets/UUtility/Prefabs/UTMP/InputField (UTMP)/InputField (UTMP).prefab";

        [MenuItem("GameObject/UT/Text - UTMP", false, 50)]
        private static void CreateUTMPText(MenuCommand command)
        {
            UTContextMenuUtility.CreateAssetFromPath(command, UTMPTextPath);
        }

        [MenuItem("GameObject/UT/Input Field - UTMP", false, 51)]
        private static void CreateUTMPInputField(MenuCommand command)
        {
            UTContextMenuUtility.CreateAssetFromPath(command, UTMPInputField);
        }
    }
}