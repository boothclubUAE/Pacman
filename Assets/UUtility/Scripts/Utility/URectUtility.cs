using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static void SetAnchorPositionX(this RectTransform rect, float x) => rect.SetAnchorPosition(x, rect.anchoredPosition.y);
        public static void SetAnchorPositionY(this RectTransform rect, float y) => rect.SetAnchorPosition(rect.anchoredPosition.x, y);
        public static void SetAnchorPositionZ(this RectTransform rect, float z) => rect.SetAnchorPosition(rect.anchoredPosition.x, rect.anchoredPosition.y, z);
        public static void SetAnchorPosition(this RectTransform rect, float x, float y) => rect.anchoredPosition = new Vector2(x, y);
        public static void SetAnchorPosition(this RectTransform rect, float x, float y, float z) => rect.anchoredPosition3D = new Vector3(x, y, z);

        public static void SetSizeDeltaX(this RectTransform rect, float x) => rect.SetSizeDelta(x, rect.sizeDelta.y);
        public static void SetSizeDeltaY(this RectTransform rect, float y) => rect.SetSizeDelta(rect.sizeDelta.x, y);
        public static void SetSizeDelta(this RectTransform rect, float x, float y) => rect.sizeDelta = new Vector2(x, y);

        public static float GetWidth(this RectTransform rect) => rect.rect.width;
        public static float GetHeight(this RectTransform rect) => rect.rect.height;
        public static Vector2 GetSize(this RectTransform rect) => new Vector2(rect.GetWidth(), rect.GetHeight());

        public static void FitTextureInside(this RawImage rawImage, Texture texture) => rawImage.FitTextureInside(rawImage.rectTransform, texture);
        public static void FitTextureInside(this RawImage rawImage, RectTransform parentRect, Texture texture) => rawImage.SetTexture(parentRect, texture, true);
        public static void FitTextureOutside(this RawImage rawImage, Texture texture) => rawImage.FitTextureOutside(rawImage.rectTransform, texture);
        public static void FitTextureOutside(this RawImage rawImage, RectTransform parentRect, Texture texture) => rawImage.SetTexture(parentRect, texture, false);
        public static void SetTexture(this RawImage rawImage, RectTransform parentRect, Texture texture, bool fitInside)
        {
            Vector2Int tSize = texture.GetSize();
            Vector2 size = parentRect.rect.size;

            bool preserveHeight = true;

            if (fitInside)
                preserveHeight = tSize.x <= tSize.y;
            else
                preserveHeight = tSize.y < tSize.x;

            size = size.PreserveAspectRatio(tSize, preserveHeight);
            tSize = ResizeWithAspectRatio(tSize.x, tSize.y, (int)size.x, (int)size.y);

            rawImage.rectTransform.sizeDelta = tSize;
            rawImage.texture = texture;
        }

        public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
            deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
            deltaPosition.Scale(rectTransform.localScale);          // apply scaling
            deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

            rectTransform.pivot = pivot;                            // change the pivot
            rectTransform.localPosition -= deltaPosition;           // reverse the position change
        }

        public static void SetAnchors(this RectTransform This, Vector2 AnchorMin, Vector2 AnchorMax)
        {
            var OriginalPosition = This.localPosition;
            var OriginalSize = This.sizeDelta;

            This.anchorMin = AnchorMin;
            This.anchorMax = AnchorMax;

            This.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, OriginalSize.x);
            This.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, OriginalSize.y);
            This.localPosition = OriginalPosition;
        }

        public static Vector2 GetPivotPosition(this RectTransform rect)
        {
            return rect.sizeDelta * rect.pivot;
        }

        public static Vector2 ToAnchoredPosition(this Vector2 vector, RectTransform rect)
        {
            return (vector - rect.GetPivotPosition()) * rect.localScale;
        }
    }
}