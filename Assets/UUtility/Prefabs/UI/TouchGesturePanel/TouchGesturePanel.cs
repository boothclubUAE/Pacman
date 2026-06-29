using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TouchGesturePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int tapCount = 0;
    [SerializeField] private float tapVaildDuration = 0.3f;
    [SerializeField] private float swipeVaildDuration = 0.3f;
    [SpaceArea]
    [SerializeField] private GestureHandlerRect activeHandler;
    [SpaceArea]
    [SerializeField] private GestureState dynamicSwipeState = GestureState.None;
    [SpaceArea]
    [SerializeField][IgnoreParent] private Pointer tgPointer = new Pointer();
    [SpaceArea]
    [SerializeField] public UnityEvent<int, Vector2> OnTap = new UnityEvent<int, Vector2>();
    [SerializeField] public UnityEvent<GestureState, float> OnDynamicSwipe = new UnityEvent<GestureState, float>();
    [SpaceArea]
    [SerializeField] public UnityEvent OnSwipeLeft = new UnityEvent();
    [SerializeField] public UnityEvent OnSwipeRight = new UnityEvent();
    [SerializeField] public UnityEvent OnSwipeUp = new UnityEvent();
    [SerializeField] public UnityEvent OnSwipeDown = new UnityEvent();

    private PointerEventData tappingPointer;
    private float lastTapTime;
    private float lastTappedTime;

    private float lastSwipeBeginTime;

    private void Awake()
    {
        if (activeHandler)
            SetHandler(activeHandler);
    }

    public void SetHandler(GestureHandlerRect handler)
    {
        if (handler.handleGestures)
        {
            activeHandler = handler;
            activeHandler.SetPointer(tgPointer);
        }
    }

    public void RemoveHandler()
    {
        activeHandler = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tgPointer.AddSubPointer(eventData))
        {
            StartTap(eventData);     
            tgPointer.Begin();
            OnSinglePointerBegin();
        }
        else
        {
            tappingPointer = null;
            OnMultiPointerBegin();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (tgPointer.RemoveSubPointer(eventData))
        {
            OnSinglePointerEnd();
            CheckForTapping(eventData);
            tgPointer.End();
        }
        else
            OnMultiPointerEnd();
    }

    private void StartTap(PointerEventData tapPointer)
    {
        tappingPointer = tapPointer;
        lastTapTime = Time.time;
    }

    private void CheckForTapping(PointerEventData tapPointer)
    {
        if (tappingPointer == null)
            return;

        if (tappingPointer.pointerId == tappingPointer.pointerId)
        {
            float tapTime = Time.time - lastTapTime;

            if (tapTime <= tapVaildDuration)
            {
                float tappedTime = Time.time - lastTappedTime;

                if (tappedTime > tapVaildDuration)
                    tapCount = 0;

                lastTappedTime = Time.time;
                tapCount++;

                if (activeHandler && activeHandler.active && activeHandler.IsPointInRect(tapPointer.position))
                    activeHandler.OnTap(tapCount, tapPointer.position);

                OnTap?.Invoke(tapCount, tapPointer.position);
                return;
            }
        }

        ResetTap();
    }

    private void ResetTap()
    {
        tappingPointer = null;

        lastTapTime = 0;
        lastTappedTime = 0;
        tapCount = 0;
    }

    public void ResetGestureHandler()
    {
        if(activeHandler)
            if(activeHandler.active)
                activeHandler.SetActive(false);
    }

    private void OnSinglePointerBegin()
    {
        if (activeHandler && activeHandler.active && activeHandler.IsPointInRect(tgPointer.position))
            activeHandler.OnPointerBegin(true);
        else
            SetDynamicSwipeState(GestureState.Begin);

        lastSwipeBeginTime = Time.time;
    }

    private void OnMultiPointerBegin()
    {
        if (activeHandler && activeHandler.active && activeHandler.IsPointInRect(tgPointer.position))
            activeHandler.OnPointerBegin(false);

        //if (activeHandler)
        //{
        //    SetDynamicSwipeState(GestureState.Cancel);
        //    activeHandler.Begin();
        //}
    }

    private void Update()
    {
        if (tgPointer.active)
        {
            tgPointer.Update();
            ZoomAndPan();
            DynamicSwipe();
        }
    }

    private void OnSinglePointerEnd()
    {
        if (activeHandler && activeHandler.active)
            activeHandler.OnPointerEnd(true);
        else
            SetDynamicSwipeState(GestureState.End);

        if (Time.time - lastSwipeBeginTime > swipeVaildDuration)
            return;

        SwipeDir dir = Swipe();

        switch (dir)
        {
            case SwipeDir.Left:
                OnSwipeLeft?.Invoke();
                break;

            case SwipeDir.Right:
                OnSwipeRight?.Invoke();
                break;

            case SwipeDir.Up:
                OnSwipeUp?.Invoke();
                break;

            case SwipeDir.Down:
                OnSwipeDown?.Invoke();
                break;
        }
    }

    private void OnMultiPointerEnd()
    {
        if (activeHandler && activeHandler.active)
            activeHandler.OnPointerEnd(false);
    }

    private void ZoomAndPan()
    {
        if (!activeHandler)
            return;

        if (!activeHandler.active)
        {
            if (tgPointer.deltaScale != 0f)
            {
                activeHandler.SetActive(true);

                SetDynamicSwipeState(GestureState.Cancel);
            }
            else
            {
                //if (dynamicSwipeState == GestureState.Cancel)
                //    SetDynamicSwipeState(GestureState.Begin);
            }
        }
        else
            activeHandler.Process();
    }

    private void SetDynamicSwipeState(GestureState gestureState, float value = 0)
    {
        if (dynamicSwipeState == gestureState)
            return;

        dynamicSwipeState = gestureState;

        if (gestureState == GestureState.Begin)
            if (activeHandler)
                if (activeHandler.active)
                    activeHandler.SetActive(false);

        OnDynamicSwipe?.Invoke(dynamicSwipeState, value);
    }

    private void DynamicSwipe()
    {
        if (dynamicSwipeState == GestureState.None || dynamicSwipeState == GestureState.Cancel)
            return;

        Vector2 deltaPosition = tgPointer.deltaPosition;
        SwipeDir dir = Swipe();

        float value = dir switch
        {
            SwipeDir.Left => Mathf.Abs(deltaPosition.x),
            SwipeDir.Right => -Mathf.Abs(deltaPosition.x),
            SwipeDir.Up => Mathf.Abs(deltaPosition.y),
            SwipeDir.Down => -Mathf.Abs(deltaPosition.y),
            _ => Mathf.Abs(deltaPosition.x)
        };

        OnDynamicSwipe?.Invoke(GestureState.Update, value);

        //SetDynamicSwipeState(GestureState.Update, value);
    }

    private SwipeDir Swipe()
    {
        Vector2 deltaPosition = tgPointer.deltaPosition;
        deltaPosition.Normalize();

        if (Mathf.Abs(deltaPosition.x) >= Mathf.Abs(deltaPosition.y))
        {
            if (deltaPosition.x == 0)
                return SwipeDir.None;
            else
                return deltaPosition.x < 0 ? SwipeDir.Right : SwipeDir.Left;
        }
        else
        {
            if (deltaPosition.y == 0)
                return SwipeDir.None;
            else
                return deltaPosition.y > 0 ? SwipeDir.Up : SwipeDir.Down;
        }
    }

    [System.Serializable]
    public class Pointer
    {
        [BeginGroup] public bool active = false;
        [SpaceArea]
        public int subPointerCount;
        [SpaceArea]
        [BeginGroup] public Vector2 initialPosition = Vector2.zero;
        public Vector2 startPosition = Vector2.zero;
        public Vector2 position = Vector2.zero;
        public Vector2 centerPosition = Vector2.zero;
        [SpaceArea]
        public Vector2 deltaPosition = Vector2.zero;
        [EndGroup] public Vector2 centerDeltaPosition = Vector2.zero;
        [SpaceArea]
        public float initialPDistance;
        public float pDistance;
        [EndGroup] public float deltaScale;

        private PointerEventData primarySubPointer;

        private Dictionary<int, PointerEventData> subPointers = new Dictionary<int, PointerEventData>();
        private List<PointerEventData> subPointerList = new List<PointerEventData>();

        public bool AddSubPointer(PointerEventData pointer)
        {
            if (!subPointers.ContainsKey(pointer.pointerId))
            {
                subPointers.Add(pointer.pointerId, pointer);
                subPointerList.Add(pointer);
            }

            subPointerCount = subPointers.Count;

            if (subPointerCount == 1)
            {
                primarySubPointer = pointer;

                startPosition = pointer.position;
                initialPosition = startPosition;
            }

            Update();

            if (subPointerCount >= 2)
            {
                initialPDistance = pDistance;
                UpdateDeltaScale();
            }

            return subPointerCount == 1;
        }

        public bool RemoveSubPointer(PointerEventData pointer)
        {
            if (subPointers.ContainsKey(pointer.pointerId))
            {
                subPointerList.Remove(subPointers[pointer.pointerId]);
                subPointers.Remove(pointer.pointerId);
            }

            subPointerCount = subPointers.Count;

            if (subPointerCount == 0)
            {

            }
            else
            {
                if (primarySubPointer.pointerId == pointer.pointerId)
                {
                    startPosition = subPointerList[0].position - deltaPosition;
                    //offsetPosition = subPointerList[0].position + primarySubPointer.position;
                    primarySubPointer = subPointerList[0];
                }

                Update();

                initialPDistance = pDistance;
            }

            return subPointerCount == 0;
        }

        public void Begin()
        {
            active = true;
        }

        public void Update()
        {
            position = subPointerList[0].position;
            deltaPosition = position - startPosition;

            centerPosition = Vector2.zero;
            foreach (PointerEventData pointer in subPointerList)
                centerPosition += pointer.position;
            centerPosition /= subPointerCount;

            centerDeltaPosition = centerPosition - startPosition;

            UpdateDeltaScale();
        }

        private void UpdateDeltaScale()
        {
            if (subPointerCount == 1)
            {

            }
            else
            {
                pDistance = 0;
                foreach (PointerEventData pointer in subPointerList)
                    pDistance += Vector2.Distance(centerPosition, pointer.position);
                pDistance /= subPointerCount;

                deltaScale = pDistance - initialPDistance;
                deltaScale /= 200;
            }
        }

        public void End()
        {
            active = false;

            initialPosition = Vector2.zero;
            startPosition = Vector2.zero;
            position = Vector2.zero;
            centerPosition = Vector2.zero;
            deltaPosition = Vector2.zero;
            centerDeltaPosition = Vector2.zero;

            initialPDistance = 0;
            pDistance = 0;
            deltaScale = 0;
        }
    }

    private enum SwipeDir
    {
        Left,
        Right,
        Up,
        Down,
        None
    }

    public enum GestureState
    {
        None,
        Begin,
        Update,
        End,
        Cancel
    }
}
