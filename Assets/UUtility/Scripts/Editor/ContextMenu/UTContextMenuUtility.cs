using UnityEngine;
using UnityEditor;

namespace UTool.Editor
{
    public static class UTContextMenuUtility
    {
        public static GameObject CreateAssetFromPath(MenuCommand command, string assetPath, bool prefabLink = false, bool select = true)
        {
            GameObject selectedGO = (GameObject)command.context;
            Transform selectedTransform = selectedGO ? selectedGO.transform : (Transform)command.context;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject gameObject;

            if (prefabLink)
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, selectedTransform);
            else
                gameObject = GameObject.Instantiate(prefab, selectedTransform);

            gameObject.name = prefab.name;

            Undo.RegisterCreatedObjectUndo(gameObject, "Created " + gameObject.name);

            if (select)
                Selection.activeGameObject = gameObject;

            return gameObject;
        }
    }
}