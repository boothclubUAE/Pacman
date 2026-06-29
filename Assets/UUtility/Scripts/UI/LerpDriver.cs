using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpDriver : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SpaceArea]
    [SerializeField][Disable] private Vector2 startValue;
    [SerializeField] private Vector2 endValue;

    private void Awake()
    {
        startValue = rect.sizeDelta;
    }

    public void LerpUpdate(int stage, float value)
    {
        if (stage == 2)
            value = 0;

        rect.sizeDelta = Vector2.Lerp(startValue, endValue, value);
    }
}
