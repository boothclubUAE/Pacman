using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using DG.Tweening;
using UTool.Utility;

using static RingImage;

public class HiddenButtonPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float startDelay = 0;
    [SerializeField] private float holdDuration = 1;
    [SpaceArea]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private CanvasGroup ringCG;
    [SerializeField] private ButtonOption buttonOptionPrefab;
    [SerializeField] private RingImage ring;
    [SpaceArea]
    [SerializeField]
    [LabelByChild("option")]
    [ReorderableList] private List<ButtonOptionCtrl> buttonOptions = new List<ButtonOptionCtrl>();
    [SpaceArea]
    [SerializeField][Disable][IgnoreParent] private ButtonOptionCtrl latestBOC;
    private
    Tween holdTween;
    Tween ringTween;
    Tween ringCGTween;

    private int currentPointer = -99;
    public bool isOpen = false;
    private bool touchDisabled = false;

    private void Awake()
    {
        float[] equallyDistributedPoints = GetEquallyDistributedPoints(20, 80, buttonOptions.Count);

        for (int i = 0; i < equallyDistributedPoints.Length; i++)
        {
            ButtonOptionCtrl buttonOptionCtrl = buttonOptions[i];
            float offset = equallyDistributedPoints[i];

            ButtonOption bo = Instantiate(buttonOptionPrefab, ring.transform);
            bo.display.text = buttonOptionCtrl.option;
            bo.boc = buttonOptionCtrl;
            
            bo.OnKeyEnter = (value) => latestBOC = value;
            bo.OnKeyExit = (value) =>
            {
                if (latestBOC == value)
                    latestBOC = null;
            };

            bo.OnKeyUp = (value) =>
            {
                OnRelease();
            };

            if (buttonOptionCtrl.usePrimaryFont)
                bo.UsePrimaryFont();

            RingChildTransform RCT = new RingChildTransform();
            RCT.childRect = bo.rect;
            RCT.pivot = new Vector2(0, 0.5f);
            RCT.offset.x = offset;

            ring.ringChildTransforms.Add(RCT);
        }
    }

    public void DisableTouch(bool state)
    {
        touchDisabled = state;
        cg.blocksRaycasts = !state;
    }

    private void TempDisableTouch(bool state)
    {
        cg.blocksRaycasts = !state;
    }

    public static float[] GetEquallyDistributedPoints(float startPoint, float endPoint, int numPoints)
    {
        float[] points = new float[numPoints];

        if (numPoints == 1)
        {
            points[0] = (startPoint + endPoint) / 2;
        }
        else
        {
            float interval = (endPoint - startPoint) / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                points[i] = startPoint + interval * i;
            }
        }

        return points;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (currentPointer != -99)
            return;

        currentPointer = eventData.pointerId;
        StartHold();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        CancelHold(eventData.pointerId);

        OnRelease();
    }

    private void OnRelease()
    {
        if (latestBOC != null)
        {
            OpenRing(false, latestBOC.forceClose);

            latestBOC.OnOptionSelected.Invoke();
            return;
        }

        OpenRing(false);
    }

    private void StartHold()
    {
        latestBOC = null;

        holdTween = DOVirtual.Float(0, 1, holdDuration, HoldUpdate)
            .SetEase(Ease.Linear)
            .SetDelay(startDelay)
            .OnComplete(HoldEnd);
    }

    private void CancelHold(int id)
    {
        if (currentPointer != id)
            return;

        currentPointer = -99;

        holdTween.KillTween();
    }

    private void HoldUpdate(float value)
    {

    }

    private void HoldEnd()
    {
        OpenRing(true);
    }

    public void Toggle()
    {
        OpenRing(!isOpen);
    }

    public void OpenRing(bool state, bool instant = false)
    {
        ringTween.KillTween();
        ringCGTween.KillTween();

        isOpen = state;

        if(touchDisabled)
            TempDisableTouch(!isOpen);

        if (instant)
        {
            ring.ringPivot = state ? 0 : 1;
            ring.UpdateTransform();

            ringCG.alpha = state ? 1 : 0;

            return;
        }

        ringTween = DOVirtual.Float(ring.ringPivot, state? 0 : 1, 0.3f,
            (value) =>
            {
                ring.ringPivot = value;
                ring.UpdateTransform();
            });

        ringCGTween = ringCG.FadeCanvasGroup(state);
    }

    [System.Serializable]
    public class ButtonOptionCtrl
    {
        [BeginGroup]
        public string option;
        public bool forceClose = false;
        public bool usePrimaryFont = false;
        [SpaceArea]
        [EndGroup]
        public UnityEvent OnOptionSelected = new UnityEvent();
    }
}