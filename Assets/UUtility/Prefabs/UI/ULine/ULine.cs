using System;
using UnityEngine;

using UTool.Utility;

using DG.Tweening;

public class ULine : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform startCap;
    [SerializeField] private RectTransform endCap;
    [SerializeField] private CanvasGroup CG;
    [SpaceArea]
    [SerializeField] private float width;
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;
    [SerializeField] private float distance;

    public void Draw(float width, Vector2 startPoint, Vector2 endPoint)
    {
        this.width = width;
        startCap.sizeDelta = width * Vector2.one;
        endCap.sizeDelta = width * Vector2.one;

        this.startPoint = startPoint;
        rect.anchoredPosition = startPoint;

        this.endPoint = endPoint;
        distance = Vector2.Distance(startPoint, endPoint);

        rect.SetSizeDeltaX(distance);

        LookAtTargetPoint();
    }

    private void LookAtTargetPoint()
    {
        Vector2 dir = endPoint - startPoint;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
