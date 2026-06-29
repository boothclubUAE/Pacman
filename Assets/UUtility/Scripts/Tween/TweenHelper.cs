using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTool.Tweening.Helper
{
    public static class TweenHelper
    {
        public static void UpdateProperty(RectTransform tweenRect, TweenPropertyType propertyType, Vector3 value)
        {
            if (!tweenRect)
                return;

            switch (propertyType)
            {
                case TweenPropertyType.Position:
                    tweenRect.anchoredPosition = value;
                    break;

                case TweenPropertyType.Scale:
                    tweenRect.localScale = value;
                    break;

                case TweenPropertyType.Rotation:
                    tweenRect.localRotation = Quaternion.Euler(value);
                    break;

                case TweenPropertyType.Size:
                    tweenRect.sizeDelta = value;
                    break;

                case TweenPropertyType.Pivot:
                    tweenRect.pivot = value;
                    break;
            }
        }

        public static void UpdateProperty(Transform tweenTransform, TweenPropertyType propertyType, Vector3 value)
        {
            if (!tweenTransform)
                return;

            switch (propertyType)
            {
                case TweenPropertyType.Position:
                    tweenTransform.localPosition = value;
                    break;

                case TweenPropertyType.Scale:
                    tweenTransform.localScale = value;
                    break;

                case TweenPropertyType.Rotation:
                    tweenTransform.localRotation = Quaternion.Euler(value);
                    break;
            }
        }
    }
}