using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{
    public static class FindInGraphButton
    {
        [MenuItem("GameObject/UT/EventGraph/Find in current graph", false, 1000)]
        static void FindEvents()
        {
            EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
            if(window != null)
            {
                window.OverrideSelection(Selection.activeInstanceID);
            }
        }

		[MenuItem("GameObject/UT/EventGraph/Graph just this", false, 1001)]
		static void GraphSelection()
		{
			EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
			window.RebuildGraph(new GameObject[] { Selection.activeGameObject }, false);
		}
		[MenuItem("GameObject/UT/EventGraph/Graph this hierarchy", false, 1002)]
		static void GraphSelectionHierarchy()
		{
			EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
			window.RebuildGraph(new GameObject[] { Selection.activeGameObject }, true);
		}
	}

}
