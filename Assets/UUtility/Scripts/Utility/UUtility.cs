using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable.Count() == 0;
        public static bool HasIndex<T>(this IEnumerable<T> enumerable, int index)
            => enumerable.IsEmpty() ? false : index >= 0 && index < enumerable.Count();

        public static bool Has<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
            => enumerable.Where(predicate).Count() != 0;

        public static T Random<T>(this List<T> list)
            => list[UnityEngine.Random.Range(0, list.Count())];

        public static void Shuffle<T>(this List<T> list)
        {
            List<T> tempList = new List<T>(list);
            list.Clear();
            while (tempList.Count() > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempList.Count());
                T item = tempList[randomIndex];
                tempList.Remove(item);
                list.Add(item);
            }
        }

        public static float Select(this Vector2 vector, bool selectX)
            => selectX ? vector.x : vector.y;

        //Monospace
        private static string MS4(bool state) => MS(0.4f, state);
        private static string MS6(bool state) => MS(0.6f, state);
        private static string MS(float space, bool state)
            => state ? $"<mspace={space}em>" : "";
        private static string MSC(bool state)
            => state ? $"</mspace>" : "";

        public static string ToHourMinSec(this TimeSpan ts, bool monoTag = true)
            => $"{MS6(monoTag)}{ts.Hours.ToString("00")}{MS4(monoTag)}:{MS6(monoTag)}{ts.Minutes.ToString("00")}{MS4(monoTag)}:{MS6(monoTag)}{ts.Seconds.ToString("00")}{MSC(monoTag)}";

        public static string ToMinSec(this TimeSpan ts, bool monoTag = true)
            => $"{MS6(monoTag)}{ts.Minutes.ToString("00")}{MS4(monoTag)}:{MS6(monoTag)}{ts.Seconds.ToString("00")}{MSC(monoTag)}";

        public static string ToSecMill(this TimeSpan ts, bool monoTag = true)
            => $"{MS6(monoTag)}{ts.Seconds.ToString("00")}{MS4(monoTag)}.{MS6(monoTag)}{ts.Milliseconds.ToString("000")}{MSC(monoTag)}";

        public static string ToMinSecMill(this TimeSpan ts, bool monoTag = true)
            => $"{MS6(monoTag)}{ts.Minutes.ToString("00")}{MS4(monoTag)}:{MS6(monoTag)}{ts.Seconds.ToString("00")}{MS4(monoTag)}.{MS6(monoTag)}{ts.Milliseconds.ToString("000")}{MSC(monoTag)}";

        public static string ToHexString(this string binaryString)
        {
            int decimalNumber = Convert.ToInt32(binaryString, 2);
            string hexadecimalNumber = Convert.ToString(decimalNumber, 16);

            return hexadecimalNumber;
        }

        public static Vector2 PreserveAspectRatio(this Vector2 size, Vector2 aspectRatio, bool preserveHeight = true)
        {
            if (preserveHeight)
                size.x = size.y * (aspectRatio.x / aspectRatio.y);
            else
                size.y = size.x * (aspectRatio.y / aspectRatio.x);

            return size;
        }

        public static List<T> GetChilds<T>(this Transform transform)
        {
            List<T> childComponent = new List<T>();
            for(int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);

                if(childTransform.TryGetComponent<T>(out T component))
                    childComponent.Add(component);
            }

            return childComponent;
        }

        public static List<Transform> GetChilds(this Transform transform)
        {
            List<Transform> childTransforms = new List<Transform>();
            for(int i = 0; i < transform.childCount; i++)
                childTransforms.Add(transform.GetChild(i));

            return childTransforms;
        }

        public static void DestroyAllChild(this Transform transform)
        {
            foreach(Transform childTransform in transform.GetChilds())
                GameObject.Destroy(childTransform.gameObject);
        }

        public static void DestroyAllChildImmediate(this Transform transform)
        {
            foreach(Transform childTransform in transform.GetChilds())
                GameObject.DestroyImmediate(childTransform.gameObject);
        }

        ///https://github.com/ManeFunction/unity-mane/tree/master
        /// <summary>
        /// Tries to stop a coroutine attached to a MonoBehaviour and sets the coroutine reference to null.
        /// </summary>
        /// <param name="target">The MonoBehaviour the coroutine is attached to.</param>
        /// <param name="coroutine">The coroutine to stop.</param>
        /// <returns>True if the coroutine was not null and the MonoBehaviour exists, false otherwise.</returns>
        public static bool TryKillCoroutine(this MonoBehaviour target, ref Coroutine coroutine)
        {
            if (coroutine == null || !target) return false;

            target.StopCoroutine(coroutine);
            coroutine = null;

            return true;
        }

        public static bool IsPrefabInScene(this UnityEngine.Object prefab)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular)
                if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.Connected)
                    return true;
#endif
            return false;
        }

        public static bool IsPrefabSceneView()
        {
#if UNITY_EDITOR
            return PrefabStageUtility.GetCurrentPrefabStage();
#else
            return false;
#endif
        }

        public static void RecordPrefabChanges(this UnityEngine.Object uObject)
        {
#if UNITY_EDITOR
            PrefabUtility.RecordPrefabInstancePropertyModifications(uObject);
#endif
        }

        public static void ForceRecordPrefabChanges(this UnityEngine.Object uObject)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(uObject);
#endif
        }

        public static void ApplyPrefabChanges(this UnityEngine.Object uObject)
        {
#if UNITY_EDITOR
            PrefabUtility.ApplyObjectOverride(uObject, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(uObject), InteractionMode.AutomatedAction);
#endif
        }

        public static void ApplyAllPrefabChanges(this GameObject gameObject)
        {
#if UNITY_EDITOR
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
#endif
        }
    }
}