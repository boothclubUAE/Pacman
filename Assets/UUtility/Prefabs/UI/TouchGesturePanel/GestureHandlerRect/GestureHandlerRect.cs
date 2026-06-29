using UnityEngine;
using DG.Tweening;

using UTool.Utility;
using UTool;


public class GestureHandlerRect : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform contentRect;
    [SpaceArea]
    [SerializeField] public bool handleGestures = true;
    [SerializeField] public bool active = false;
    [SpaceArea]
    [SerializeField] public bool canInteract = false;
    [SpaceArea]
    [SerializeField] public Vector2 targetPosition;
    [SerializeField] public Vector2 currentPosition;
    [SpaceArea]
    [SerializeField] public Vector2 scaleLimit = new Vector2(1, 2);
    [SerializeField] public float targetScale = 1;
    [SerializeField] public float initialScale = 1;
    [SerializeField] public float currentScale = 1;
    [SpaceArea]
    [SerializeField] private float positionDuration = 0.4f;
    [SerializeField] private Ease positionEase = Ease.OutQuart;
    [SpaceArea]
    [SerializeField] private float scaleDuration = 0.4f;
    [SerializeField] private Ease scaleEase = Ease.OutQuart;
    [SpaceArea]
    [SerializeField][IgnoreParent] private TouchGesturePanel.Pointer pointer;

    Tween positionTween;
    Tween scaleTween;

    public void SetPointer(TouchGesturePanel.Pointer pointer)
    {
        this.pointer = pointer;
    }

    public bool IsPointInRect(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, UT._instance.sceneCameras[0]);
    }

    public void SetActive(bool state)
    {
        active = state;

        if (state)
            OnPointerBegin(true);
        else
        {
            ScaleTo(scaleLimit.x);
            GoTo(contentRect.anchoredPosition);
        }
    }

    public void OnTap(int tapCount, Vector2 tapPosition)
    {
        if (tapCount == 2)
            SetActive(false);
    }

    public void OnPointerBegin(bool initialPointer)
    {
        if (!active)
            return;

        SetPivotToPointer(pointer.centerPosition);
        initialScale = currentScale;

        canInteract = true;
        Process(true);
    }

    public void Process(bool instant = false)
    {
        if (!canInteract)
            return;

        float newScale = initialScale + pointer.deltaScale;
        newScale = newScale.ClampMin(scaleLimit.x);
        //newScale = newScale.ClampMin(scaleLimit.x - (scaleLimit.x / 10));

        Vector2 newPosition = contentRect.anchoredPosition + GetPositionInsideRect(contentRect, pointer.centerPosition);

        ScaleTo(newScale, instant);
        GoTo(newPosition, instant);
    }

    public void OnPointerEnd(bool initialPointer)
    {
        if (!active)
            return;

        if (initialPointer)
        {
            canInteract = false;

            if (currentScale < scaleLimit.x)
                ScaleTo(scaleLimit.x);
            else if (currentScale > scaleLimit.y)
                ScaleTo(scaleLimit.y);

            GoTo(contentRect.anchoredPosition);
            //GoTo(contentRect.anchoredPosition + (contentRect.sizeDelta  * 0.5f).ToAnchoredPosition(contentRect));

            if (targetScale == scaleLimit.x)
                SetActive(false);
        }
        else
        {
            SetPivotToPointer(pointer.centerPosition);
            Process(true);
        }
    }

    private Vector2 GetPositionInsideRect(RectTransform rect, Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition, UT._instance.sceneCameras[0], out Vector2 localP);
        return localP;
    }

    private void SetPivotToPointer(Vector2 pointerPosition)
    {
        contentRect.SetPivot(Vector2.zero);
        Vector2 pointInRect = GetPositionInsideRect(contentRect, pointerPosition);
        contentRect.SetPivot(new Vector2(UUtility.RangedMapUnClamp(pointInRect.x, 0, contentRect.sizeDelta.x, 0, 1), UUtility.RangedMapUnClamp(pointInRect.y, 0, contentRect.sizeDelta.y, 0, 1)));
    }

    private void ScaleTo(float scale, bool instant = false)
    {
        scaleTween.KillTween();

        targetScale = scale;

        if (instant)
        {
            UpdateScale(scale);
            return;
        }

        scaleTween = DOVirtual.Float(currentScale, scale, scaleDuration, UpdateScale)
            .SetEase(scaleEase);

        void UpdateScale(float newScale)
        {
            currentScale = newScale;
            contentRect.localScale = Vector2.one * currentScale;
        }
    }

    private void GoTo(Vector2 position, bool instant = false)
    {
        positionTween.KillTween();

        //position += GetLimitOffset();
        targetPosition = position;

        if (instant)
        {
            UpdatePosition(position);
            return;
        }

        positionTween = DOVirtual.Vector2(currentPosition, position, positionDuration, UpdatePosition)
            .SetEase(positionEase);

        void UpdatePosition(Vector2 newPosition)
        {
            currentPosition = newPosition;
            contentRect.anchoredPosition = currentPosition;
            contentRect.anchoredPosition += GetLimitOffset();

            //if (currentScale <= scaleLimit.x)
            //    contentRect.anchoredPosition += GetLimitOffset();
        }

        Vector2 GetLimitOffset()
        {
            Vector2 contentOffset = Vector2.zero;

            Vector2 contentPosX = contentRect.anchoredPosition + Vector2.zero.ToAnchoredPosition(contentRect);
            Vector2 contentPosY = contentPosX + (contentRect.sizeDelta * contentRect.localScale);

            if (contentPosX.x > 0)
                contentOffset.x = -contentPosX.x;

            if (contentPosY.x < rect.rect.size.x)
                contentOffset.x = rect.rect.size.x - contentPosY.x;

            if (contentPosX.y > 0)
                contentOffset.y = -contentPosX.y;

            if (contentPosY.y < rect.rect.size.y)
                contentOffset.y = rect.rect.size.y - contentPosY.y;

            return contentOffset;
        }
    }
}
