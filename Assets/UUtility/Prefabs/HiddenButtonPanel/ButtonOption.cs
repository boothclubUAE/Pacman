using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System;
using UnityEngine.EventSystems;

using static HiddenButtonPanel;

public class ButtonOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] public RectTransform rect;
    [SerializeField] public TextMeshProUGUI display;
    [SpaceArea]
    [SerializeField] public TMP_FontAsset primartFont;
    [SpaceArea]
    [SerializeField][IgnoreParent][Disable] public ButtonOptionCtrl boc;

    public Action<ButtonOptionCtrl> OnKeyEnter;
    public Action<ButtonOptionCtrl> OnKeyExit;
    public Action<ButtonOptionCtrl> OnKeyUp;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnKeyEnter?.Invoke(boc);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnKeyEnter?.Invoke(boc);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnKeyUp?.Invoke(boc);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnKeyExit?.Invoke(boc);
    }

    public void UsePrimaryFont()
    {
        display.font = primartFont;
        display.UpdateFontAsset();
    }
}
