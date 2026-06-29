namespace UnityToolBox.BuildTools
{
    using UnityEditor;
    using UnityEngine;

    public partial class BuildToolsEditorWindow
    {
        private const string AndroidApkExtension = ".apk";
        private const string AndroidAabExtension = ".aab";

        [SerializeField] private AndroidExtension activeAndroidExtension = AndroidExtension.Aab;
        private string keyStoreName = string.Empty;
        private string keystorePassword = string.Empty;
        private string keystoreAliasName = string.Empty;
        private string keystoreAliasPassword = string.Empty;

        private enum AndroidExtension
        {
            Apk,
            Aab,
        }

        private void LoadAndroidSpecificOptions()
        {
            activeAndroidExtension = (AndroidExtension)EditorPrefs.GetInt("_selectedAndroidExtension", (int)activeAndroidExtension);
        }

        private void ListAndroidSpecificOptions()
        {
            EditorGUI.BeginChangeCheck();

            activeAndroidExtension = (AndroidExtension)EditorGUILayout.EnumPopup("Extension", activeAndroidExtension);
            keyStoreName = EditorGUILayout.TextField("Keystore name", keyStoreName);
            keystorePassword = EditorGUILayout.PasswordField("Keystore password", keystorePassword);
            keystoreAliasName = EditorGUILayout.TextField("Keystore alias name", keystoreAliasName);
            keystoreAliasPassword = EditorGUILayout.PasswordField("Keystore alias password", keystoreAliasPassword);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt("_selectedAndroidExtension", (int)activeAndroidExtension);
            }
        }

        private void ExecuteAndroidSpecificPreProcess()
        {
            switch (activeAndroidExtension)
            {
                case AndroidExtension.Apk:
                    buildExtension = AndroidApkExtension;
                    break;

                case AndroidExtension.Aab:
                    buildExtension = AndroidAabExtension;
                    break;

                default:
                    buildExtension = AndroidAabExtension;
                    break;
            }
        }

        private void ExecuteAndroidSpecificPostProcess()
        {
            keyStoreName = string.Empty;
            keystorePassword = string.Empty;
            keystoreAliasName = string.Empty;
            keystoreAliasPassword = string.Empty;
        }
    }
}
