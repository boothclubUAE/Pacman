using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerButton : MonoBehaviour
{
    [SerializeField] public CanvasGroup canvasGroup;
    [SpaceArea]
    [SerializeField] public float inactiveDuration;
    [SpaceArea]
    [SerializeField] private bool blocked;
    [SpaceArea]
    [SerializeField] private UnityEvent OnPress = new UnityEvent();

    public void OnButtonPress()
    {
        if (blocked)
            return;

        Block(true);
        DOVirtual.DelayedCall(inactiveDuration, () => Block(false));

        OnPress?.Invoke();
    }

    private void Block(bool state)
    {
        canvasGroup.interactable = !state;
        canvasGroup.blocksRaycasts = !state;
    }
}
