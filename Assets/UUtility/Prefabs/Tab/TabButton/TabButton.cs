using UnityEngine;

using UTool.Utility;

using DG.Tweening;
using TMPro;

namespace UTool.TabSystem
{
    public class TabButton : MonoBehaviour
    {
        [SerializeField][Disable] public string tabName;
        [SpaceArea]
        [SerializeField] private RectTransform background;
        [SerializeField] private TextMeshProUGUI nameDisplay;
        [SerializeField] private TabContent content;
        [SpaceArea]
        [SerializeField] private bool active;
        [SpaceArea]
        [SerializeField] private float activeRightOffset;
        [SerializeField] private float duration;
        [SerializeField] private AnimationCurve curve;

        private Tween tween;

        public TabManager tabManager;

        public void Bind(TabManager tabManager, Tab tab)
        {
            this.tabManager = tabManager;

            tabName = tab.tabName;
            content = tab.content;

            nameDisplay.text = tabName;
        }

        public void TabActive(bool state)
        {
            active = state;

            UUtility.KillTween(tween);
            tween = DOVirtual.Float(background.offsetMax.x, active ? activeRightOffset : 0, duration,
                (value) =>
                {
                    background.offsetMax = new Vector2(value, background.offsetMax.y);
                })
                .SetEase(curve);

            if (content)
                content.gameObject.SetActive(state);
        }

        public void SelectTab()
        {
            tabManager.SelectTab(this);
        }
    }
}