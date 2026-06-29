using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Linq;

namespace UTool.Editor
{
    [InitializeOnLoad]
    public class UTEditor
    {
        //https://docs.unity3d.com/Manual/upm-api.html
        //https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html

        private static ListRequest listRequest;
        private static ListRequest installRequest;

        static UTEditor()
        {
            //listRequest = Client.List();
            //EditorApplication.update += Progress;
        }

        private static void InstallMissingPackages(List<string> installedPackages)
        {
            //installRequest = Client.Add();
            //EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                    InstallMissingPackages(listRequest.Result.Select(x => x.name).ToArray().ToList());
                else if (listRequest.Status >= StatusCode.Failure)
                    Debug.Log($"UT Editor : {listRequest.Error.message}");

                EditorApplication.update -= Progress;
            }
        }
    }
}