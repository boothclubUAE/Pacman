using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UTool.Utility;
using UTool.Tweening.Helper;

using DG.Tweening;

namespace UTool.Tweening
{
    public class TweenElement : MonoBehaviour
    {
        [SerializeField][BeginGroup] public TransformType transformType;
        [SerializeField][ShowIf(nameof(isRectTransform))] private RectTransform tweenRect;
        [SerializeField][EndGroup][ShowIf(nameof(isTransform))] private Transform tweenTransform;

        [SpaceArea]
     
        [SerializeField][BeginGroup][Disable] private int timeframe = 1000;
        [SerializeField][Disable] private int currentFrame = 0;
        [SerializeField][Disable] private int loopCounter = 0;
        [SerializeField][Disable] private bool loopCompleted = false;
        [SerializeField][Disable] private PlaybackState playbackState = PlaybackState.Idle;
        [SpaceArea]
        [SerializeField][Disable] private int totalProperty = 0;
        [SerializeField][Disable] private int completedProperty = 0;
        [SerializeField][Disable][Range(0, 1)] private float progress = 0;
        [SpaceArea]
        [SpaceArea]
        [SerializeField][Disable] private int totalSubTween = 0;
        [SerializeField][Disable] private int completedSubTween = 0;
        [SerializeField][EndGroup][Disable][Range(0, 1)] private float subTweenProgress = 0;

        [SpaceArea]
        [EditorButton(nameof(PlayTween), activityType: ButtonActivityType.OnPlayMode)]
        [EditorButton(nameof(ReverseTween), activityType: ButtonActivityType.OnPlayMode)]
        [EditorButton(nameof(StopTween), activityType: ButtonActivityType.OnPlayMode)]
        [SpaceArea]
      
        [SerializeField][BeginGroup][SearchableEnum] private AutoStartTween autoStart = AutoStartTween.Disabled;
        [SerializeField][SearchableEnum] private LoopMode loopMode = LoopMode.Disabled;
        [SerializeField] private int loopCount = -1;
        [SpaceArea]
        [SerializeField] private bool useSingleTween = true;
        [SpaceArea]
        [SerializeField] private TweenConfig playTweenConfig = new TweenConfig() { delay = 0, duration = 0.3f, ease = Ease.Linear };
        [SpaceArea]
        [SerializeField][HideIf(nameof(useSingleTween))] private TweenConfig reverseTweenConfig = new TweenConfig() { delay = 0, duration = 0.3f, ease = Ease.Linear };
        [SpaceArea]
        [SerializeField] private bool invertReverseTime = false;  
        [SerializeField][EndGroup] private bool invertKeyframeReverseTime = false;

        [SpaceArea]

        [SerializeField][LabelByChild("tweenPropertyType")][ReorderableList(Foldable = true)]
        public List<TweenProperty> tweenPropertyList = new List<TweenProperty>();

        [SpaceArea]

        [EditorButton(nameof(GetChildTweenElement), activityType: ButtonActivityType.OnEditMode)]
        [SerializeField][ReorderableList(Foldable = true)] public List<TweenElement> tweenElementChilds = new List<TweenElement>();

        [SpaceArea]

        [EditorButton(nameof(FillUsingTweenElementChilds), activityType: ButtonActivityType.OnEditMode)]
        [EditorButton(nameof(EquallyDistributeTweenActionTimeframe), activityType: ButtonActivityType.OnEditMode)]
        [SerializeField][BeginGroup] private bool defaultInvertPlaybackState = false;
        [SerializeField] private bool defaultDontInvertAtStart = false;
        [SpaceArea]
        [SerializeField] private bool flipDistribution = false;
        [SerializeField] private int distributionStartPoint = 0;
        [SerializeField][EndGroup] private int distributionEndPoint = 1000;
        [SerializeField][ReorderableList(Foldable = true)]
        public List<TweenAction> tweenActionList = new List<TweenAction>();

        [SpaceArea, Line(5)]

        [SerializeField][BeginGroup("Events")] public UnityEvent OnPlayRequest = new UnityEvent();
        [SerializeField] public UnityEvent OnPlayRequestComplete = new UnityEvent();
        [SpaceArea]
        [SerializeField] public UnityEvent OnReverseRequest = new UnityEvent();
        [SerializeField] public UnityEvent OnReverseRequestComplete = new UnityEvent();
        [SpaceArea, Line(5)]
        [SerializeField] public UnityEvent<bool> OnRequest = new UnityEvent<bool>();
        [SerializeField][EndGroup] public UnityEvent<bool> OnRequestComplete = new UnityEvent<bool>();

        private bool isRectTransform => transformType == TransformType.RectTransform;
        private bool isTransform => transformType == TransformType.Transform;

        private bool propertyCompleted => completedProperty == totalProperty;
        private bool subTweenCompleted => completedSubTween == totalSubTween;

        private bool allPropertyTweeenCompleted => propertyCompleted && subTweenCompleted;

        private Tween tween;

        private bool isUpdateCallbackEngaged = false;
        private bool latestRequestState;

        private List<Action> onCompleteCallbacks = new List<Action>();

        private void GetChildTweenElement()
        {
            List<TweenElement> teS = new List<TweenElement>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);
                if (!t.gameObject.activeSelf)
                    continue;

                TweenElement te = t.gameObject.GetComponent<TweenElement>();

                if (!te)
                    return;

                teS.Add(te);
            }

            teS = teS.Where(x => !tweenElementChilds.Contains(x)).ToList();
            tweenElementChilds.AddRange(teS);

            this.RecordPrefabChanges();
        }

        private void FillUsingTweenElementChilds()
        {
            tweenActionList.Clear();

            foreach(TweenElement childTE in tweenElementChilds)
            {
                TweenAction tweenAction = new TweenAction();
                tweenAction.invertPlaybackState = defaultInvertPlaybackState;
                tweenAction.dontInvertAtStart = defaultDontInvertAtStart;
                tweenAction.tweenElement = childTE;

                tweenActionList.Add(tweenAction);
            }

            this.ForceRecordPrefabChanges();
        }

        private void EquallyDistributeTweenActionTimeframe()
        {
            float increment = (distributionEndPoint - distributionStartPoint) / tweenActionList.Count;
            float currentTimeFrame = distributionStartPoint + (increment / 2);

            for (int i = 0; i < tweenActionList.Count; i++)
            {
                int index = flipDistribution ? (tweenActionList.Count - 1) - i : i;
                tweenActionList[index].timeframe = (int)currentTimeFrame;
                currentTimeFrame += increment;
            }

            this.ForceRecordPrefabChanges();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            if (!tweenRect && !tweenTransform)
            {
                tweenRect = transform.GetComponent<RectTransform>();
                if (!tweenRect)
                {
                    tweenTransform = transform;
                    transformType = TransformType.Transform;
                }
            }

            UpdateCurrentValue(true);

            this.RecordPrefabChanges();
        }

        private void Awake()
        {
            SetupUpdateCallbacks();
        }

        private void SetupUpdateCallbacks()
        {
            if (isUpdateCallbackEngaged)
                return;

            isUpdateCallbackEngaged = true;

            foreach (TweenProperty tweenProperty in tweenPropertyList)
            {
                tweenProperty.OnPropertyChanged = UpdateProperty;
                tweenProperty.OnComplete = PropertyComplete;
            }

            foreach (TweenAction tAction in tweenActionList)
                if (tAction.tweenElement)
                    tAction.tweenElement.OnRequestComplete.AddListener((state) => OnSubTweenComplete());
        }

        private void Start()
        {
            AutoStart();
        }

        private void AutoStart()
        {
            foreach (TweenAction tAction in tweenActionList)
                tAction.Setup();

            switch (autoStart)
            {
                case AutoStartTween.Play:
                    PlayTween();
                    break;

                case AutoStartTween.Reverse:
                    ReverseTween();
                    break;

                case AutoStartTween.PlayInstant:
                    PlayTween(instant: true);
                    break;

                case AutoStartTween.ReverseInstant:
                    ReverseTween(instant: true);
                    break;

                default:
                    break;
            }
        }

        private void UpdateCurrentValue(bool updateFromValue = false)
        {
            if (transformType == TransformType.RectTransform)
            {
                if (!tweenRect)
                    return;
            }
            else
            {
                if (!tweenTransform)
                    return;
            }

            foreach (TweenProperty tweenProperty in tweenPropertyList)
            {
                if (transformType == TransformType.RectTransform)
                    tweenProperty.SetCurrentValue(tweenRect);
                else
                    tweenProperty.SetCurrentValue(tweenTransform);

                if (updateFromValue)
                    tweenProperty.UpdateFromValue();
            }
        }

        private void PopulateChildElements()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                TweenElement childElement = transform.GetChild(i).GetComponent<TweenElement>();
                if (!childElement)
                    continue;

                TweenAction aa = tweenActionList.Find(x => x.tweenElement == childElement);
                if (aa != null)
                    continue;

                TweenAction element = new TweenAction();
                element.tag = childElement.name;
                element.timeframe = 0;
                element.tweenElement = childElement;

                tweenActionList.Add(element);
            }

            this.RecordPrefabChanges();
        }

        private void HandleOnCompleteCallback()
        {
            playbackState = PlaybackState.Idle;

            List<Action> vaildOnCompleteCallbacks = new List<Action>(onCompleteCallbacks);
            onCompleteCallbacks.Clear();

            vaildOnCompleteCallbacks.ForEach(x => x?.Invoke());

            if (latestRequestState)
                OnPlayRequestComplete?.Invoke();
            else
                OnReverseRequestComplete?.Invoke();

            OnRequestComplete?.Invoke(latestRequestState);

            if (loopMode != LoopMode.Disabled)
            {
                if (loopCount != -1)
                    if (++loopCounter >= loopCount)
                    {
                        loopCompleted = true;
                        return;
                    }

                switch (loopMode)
                {
                    case LoopMode.PlayReverseRepeat:
                        if (latestRequestState)
                            ReverseTween();
                        else
                            PlayTween();
                        break;

                    case LoopMode.PlayRepeat:
                        ResetTween(latestRequestState);
                        if (latestRequestState)
                            PlayTween();
                        else
                            ReverseTween();
                        break;
                }
            }
        }

        private void SetupCallbacks()
        {
            onCompleteCallbacks.Clear();

            totalProperty = tweenPropertyList.Where(x => !x.disableTween).Count();
            completedProperty = 0;

            totalSubTween = tweenActionList.Where(x => x.tweenElement).Count();
            completedSubTween = 0;
        }

        private void CheckCompletion()
        {
            if (allPropertyTweeenCompleted)
                HandleOnCompleteCallback();
        }

        public void PlayTween(Action OnComplete) => PlayTween(false, OnComplete);
        public void ReverseTween(Action OnComplete) => ReverseTween(false, OnComplete);

        public void PlayTween(bool instant, Action OnComplete = null) => Show(true, false, instant, OnComplete);
        public void ReverseTween(bool instant, Action OnComplete = null) => Show(false, false, instant, OnComplete);

        public void PlayTween() => Show(true);
        public void ReverseTween() => Show(false);
        public void Show(bool state, bool force = false, bool instant = false, Action OnComplete = null)
        {
            PlaybackState requestedPlayback = state ? PlaybackState.Playing : PlaybackState.Reverse;

            if (!force)
                if (requestedPlayback == playbackState)
                    return;

            SetupUpdateCallbacks();

            KillTween();
            SetupCallbacks();

            UpdateCurrentValue(false);

            playbackState = requestedPlayback;
            //currentFrame = playbackState == PlaybackState.Playing ? 0 : timeframe;

            tweenPropertyList.ForEach(tweenProperty => tweenProperty.playbackState = PlaybackState.Ready);
            tweenActionList.ForEach(tweenAction => tweenAction.playbackState = PlaybackState.Ready);

            if (instant)
            {
                foreach (TweenAction tAction in tweenActionList)
                    if (tAction.playbackState == PlaybackState.Ready)
                    {
                        tAction.playbackState = playbackState;
                        tAction.Trigger(true);
                    }

                foreach (TweenProperty tweenProperty in tweenPropertyList.Where(x => x.playbackState != playbackState))
                    if (tweenProperty.playbackState == PlaybackState.Ready)
                        tweenProperty.Show(playbackState == PlaybackState.Playing, true);

                OnComplete?.Invoke();

                return;
            }
            else
            {
                if (loopCompleted)
                {
                    loopCounter = 0;
                    loopCompleted = false;
                }

                TweenConfig tc = playTweenConfig;
                if (!useSingleTween && playbackState == PlaybackState.Reverse)
                    tc = reverseTweenConfig;

                if(!allPropertyTweeenCompleted)
                {
                    tween = DOVirtual.Int(currentFrame, state ? timeframe : 0, tc.duration, Step)
                        .SetDelay(tc.delay)
                        .SetEase(tc.ease)
                        .OnComplete(() =>
                        {

                        });
                }
            }

            onCompleteCallbacks.Add(OnComplete);

            latestRequestState = state;

            if (latestRequestState)
                OnPlayRequest?.Invoke();
            else
                OnReverseRequest?.Invoke();

            OnRequest?.Invoke(latestRequestState);

            CheckCompletion();
        }

        private void Step(int step)
        {
            currentFrame = step;

            foreach (TweenAction tAction in tweenActionList)
            {
                if (playbackState == PlaybackState.Playing)
                {
                    if (tAction.timeframe > step)
                        continue;
                }
                else
                {
                    if (invertReverseTime)
                    {
                        if (tAction.timeframe > Mathf.Abs(step - timeframe))
                            continue;
                    }
                    else
                    {
                        if (tAction.timeframe < step)
                            continue;
                    }
                }

                if (tAction.playbackState == PlaybackState.Ready)
                {
                    tAction.playbackState = playbackState;
                    tAction.Trigger(false);
                }
            }

            foreach (TweenProperty tweenProperty in tweenPropertyList.Where(x => x.playbackState != playbackState))
            {
                if (playbackState == PlaybackState.Playing)
                {
                    if (tweenProperty.keyframe > step)
                        continue;
                }
                else
                {
                    if (invertKeyframeReverseTime)
                    {
                        if (tweenProperty.keyframe > Mathf.Abs(step - timeframe))
                            continue;
                    }
                    else
                    {
                        if (tweenProperty.keyframe < step)
                            continue;
                    }
                }

                if (tweenProperty.playbackState == PlaybackState.Ready)
                    tweenProperty.Show(playbackState == PlaybackState.Playing);
            }
        }

        public void ResetTween(bool state)
        {
            tween.KillTween();
            currentFrame = state ? 0 : 1000;
            playbackState = PlaybackState.Idle;

            tweenActionList.ForEach(tAction => tAction.ResetTween(state));
            tweenPropertyList.ForEach(tweenProperty => tweenProperty.ResetTween(state));
        }

        public void StopTween() => KillTween();
        private void KillTween()
        {
            tween.KillTween();
            playbackState = PlaybackState.Idle;

            tweenActionList.ForEach(tAction => tAction.KillTween());
            tweenPropertyList.ForEach(tweenProperty => tweenProperty.KillTween());
        }

        private void UpdateProperty(TweenPropertyType propertyType, Vector3 value)
        {
            if (transformType == TransformType.RectTransform)
                TweenHelper.UpdateProperty(tweenRect, propertyType, value);
            else
                TweenHelper.UpdateProperty(tweenTransform, propertyType, value);
        }

        private void PropertyComplete(TweenPropertyType propertyType)
        {
            completedProperty++;
            progress = UUtility.RangedMapClamp(completedProperty, 0, totalProperty, 0, 1);

            if (completedProperty == totalProperty)
                CheckCompletion();
        }

        private void OnSubTweenComplete()
        {
            completedSubTween++;
            subTweenProgress = UUtility.RangedMapClamp(completedSubTween, 0, totalSubTween, 0, 1);

            if (completedSubTween == totalSubTween)
                CheckCompletion();
        }

        [System.Serializable]
        public class TweenAction
        {
            [BeginGroup] public string tag;
            public int timeframe;
            [SpaceArea]
            public TweenElement tweenElement;
            [SpaceArea]
            [Disable] public PlaybackState playbackState = PlaybackState.Idle;
            [SpaceArea]
            public bool invertPlaybackState;
            public bool dontInvertAtStart;
            [SpaceArea]
            [EndGroup] public UnityEvent<PlaybackState> OnTrigger = new UnityEvent<PlaybackState>();

            public void Setup()
            {
                if (dontInvertAtStart)
                    return;

                if (invertPlaybackState)
                {
                    playbackState = PlaybackState.Reverse;
                    Trigger();
                }
            }

            public void Trigger(bool instant = false)
            {
                PlaybackState ps = playbackState;

                if (invertPlaybackState)
                    if (ps != PlaybackState.Idle)
                        ps = playbackState == PlaybackState.Playing ? PlaybackState.Reverse : PlaybackState.Playing;

                if (tweenElement)
                    tweenElement.Show(ps == PlaybackState.Playing, instant: instant);

                OnTrigger?.Invoke(ps);
            }

            public void ResetTween(bool state)
            {
                if (tweenElement)
                    tweenElement.ResetTween(state);
            }

            public void KillTween()
            {
                if(tweenElement)
                    tweenElement.KillTween();

                playbackState = PlaybackState.Idle;
            }
        }
    }

    [System.Serializable]
    public class TweenProperty
    {
        [BeginGroup]

        [BeginGroup] public bool disableTween = false;
        [SpaceArea]
        public TweenPropertyType tweenPropertyType = TweenPropertyType.None;
        [ShowIf(nameof(IsCavasGroup))] public CanvasGroup canvasGroup;
        public int keyframe;
        [SpaceArea]
        [Disable] public PlaybackState playbackState = PlaybackState.Idle;
        [SpaceArea]
        [EndGroup] public bool useSingleTween = true;

        [SpaceArea]

        [BeginGroup] public bool useCurrentValue = true;
        public bool useAbsoluteEndValue = false;
        public Vector3 currentValue;
        [SpaceArea]
        public Vector3 from;
        [ShowIf(nameof(useAbsoluteEndValue))] public Vector3 to;
        [EndGroup][HideIf(nameof(useAbsoluteEndValue))] public Vector3 toOffset;

        [SpaceArea]

        public TweenConfig playTweenConfig = new TweenConfig() { delay = 0, duration = 0.3f, ease = Ease.InOutQuad };
        [SpaceArea]
        [EndGroup][HideIf(nameof(useSingleTween))] public TweenConfig reverseTweenConfig = new TweenConfig() { delay = 0, duration = 0.3f, ease = Ease.InOutQuad };

        [SpaceArea]

        [BeginGroup("Event"), EndGroup] public TweenEvent tweenEvent = new TweenEvent();

        Tween tween;

        public Action<TweenPropertyType, Vector3> OnPropertyChanged;
        public Action<TweenPropertyType> OnComplete;

        private bool IsCavasGroup => tweenPropertyType == TweenPropertyType.CanvasGroup;
        private bool IsCustom => tweenPropertyType == TweenPropertyType.Custom;

        private Vector3 endValue => useAbsoluteEndValue ? to : from + toOffset;

        public void PlayTween() => Show(true);
        public void ReverseTween() => Show(false);
        public void Show(bool state, bool instant = false)
        {
            if (disableTween)
                return;

            KillTween();
            playbackState = state ? PlaybackState.Playing : PlaybackState.Reverse; ;

            TweenConfig tc = playTweenConfig;
            if (!useSingleTween && playbackState == PlaybackState.Reverse)
                tc = reverseTweenConfig;

            Vector3 targetValue = state ? endValue : from;
            bool hidding = targetValue.x == 0;

            if(instant)
            {
                OnUpdateStart(state);
                OnUpdateValue(targetValue);
                OnUpdateEnd(hidding, state);
            }
            else
            {
                tween = DOVirtual.Vector3(currentValue, targetValue, instant ? 0 : tc.duration, OnUpdateValue)
                .SetDelay(instant ? 0 : tc.delay)
                .SetEase(tc.ease)
                .OnStart(() => OnUpdateStart(state))
                .OnComplete(() => OnUpdateEnd(hidding, state));
            }
        }

        private void OnUpdateStart(bool state)
        {
            switch (tweenPropertyType)
            {
                case TweenPropertyType.CanvasGroup:
                    canvasGroup.blocksRaycasts = false;
                    //canvasGroup.interactable = false;
                    break;
            }

            tweenEvent.Started(currentValue, state);
        }

        private void OnUpdateValue(Vector3 vector)
        {
            switch (tweenPropertyType)
            {
                case TweenPropertyType.CanvasGroup:
                    canvasGroup.alpha = vector.x;
                    break;
            }

            currentValue = vector;

            tweenEvent.Updated(vector);         
            OnPropertyChanged?.Invoke(tweenPropertyType, currentValue);
        }

        private void OnUpdateEnd(bool hidding, bool state)
        {
            KillTween();

            switch (tweenPropertyType)
            {
                case TweenPropertyType.CanvasGroup:

                    if (!hidding)
                        canvasGroup.blocksRaycasts = true;
                    break;
            }

            tweenEvent.Completed(currentValue, state);
            OnComplete?.Invoke(tweenPropertyType);
        }

        public void KillTween()
        {
            tween.KillTween();
            playbackState = PlaybackState.Idle;
        }

        public void ResetTween(bool state)
        {
            KillTween();
            currentValue = state ? from : endValue;
        }

        public void SetCurrentValue(RectTransform tweenRect)
        {
            if (!useCurrentValue)
                return;

            switch (tweenPropertyType)
            {
                case TweenPropertyType.Position:
                    currentValue = tweenRect.anchoredPosition;
                    break;

                case TweenPropertyType.Scale:
                    currentValue = tweenRect.localScale;
                    break;

                case TweenPropertyType.Rotation:
                    currentValue = tweenRect.localRotation.eulerAngles;
                    break;

                case TweenPropertyType.Size:
                    currentValue = tweenRect.sizeDelta;
                    break;

                case TweenPropertyType.Pivot:
                    currentValue = tweenRect.pivot;
                    break;
            }

            SetCurrentSpecialPropertyValue();
        }

        public void SetCurrentValue(Transform tweenTransform)
        {
            if (!useCurrentValue)
                return;

            switch (tweenPropertyType)
            {
                case TweenPropertyType.Position:
                    from = tweenTransform.localPosition;
                    break;

                case TweenPropertyType.Scale:
                    from = tweenTransform.localScale;
                    break;

                case TweenPropertyType.Rotation:
                    from = tweenTransform.localRotation.eulerAngles;
                    break;
            }

            SetCurrentSpecialPropertyValue();
        }

        private void SetCurrentSpecialPropertyValue()
        {
            switch (tweenPropertyType)
            {
                case TweenPropertyType.CanvasGroup:
                    if (canvasGroup)
                        currentValue = new Vector3(canvasGroup.alpha, 0, 0);
                    break;
            }
        }

        public void UpdateFromValue()
        {
            if (!useCurrentValue)
                return;

            from = currentValue;
        }
    }

    [System.Serializable]
    public class TweenEvent
    {
        [BeginGroup] public UnityEvent OnPlay = new UnityEvent();
        public UnityEvent OnPlayComplete = new UnityEvent();
        public UnityEvent OnReverse = new UnityEvent();
        public UnityEvent OnReverseComplete = new UnityEvent();
        [SpaceArea]
        public UnityEvent<float> OnUpdateFloat = new UnityEvent<float>();
        public UnityEvent<Vector2> OnUpdateVector2 = new UnityEvent<Vector2>();
        public UnityEvent<Vector3> OnUpdateVector3 = new UnityEvent<Vector3>();
        [EndGroup] public UnityEvent<TweenEventType, Vector3> OnEvent = new UnityEvent<TweenEventType, Vector3>();

        public void Started(Vector3 vector, bool state)
        {
            if (state)
                OnPlay?.Invoke();
            else
                OnReverse?.Invoke();

            OnEvent?.Invoke(TweenEventType.Start, vector);
        }

        public void Updated(Vector3 vector)
        {
            OnUpdateFloat?.Invoke(vector.x);
            OnUpdateVector2?.Invoke(new Vector2(vector.x, vector.y));
            OnUpdateVector3?.Invoke(vector);

            OnEvent?.Invoke(TweenEventType.Update, vector);
        }

        public void Completed(Vector3 vector, bool state)
        {
            if (state)
                OnPlayComplete?.Invoke();
            else
                OnReverseComplete?.Invoke();

            OnEvent?.Invoke(TweenEventType.Completed, vector);
        }
    }

    [System.Serializable]
    public class TweenConfig
    {
        [BeginGroup] public float delay = 0;
        public float duration = 0.3f;
        [EndGroup][SearchableEnum] public Ease ease = Ease.InOutQuad;
    }

    public enum TransformType
    {
        RectTransform,
        Transform
    }

    public enum AutoStartTween
    {
        Disabled,
        Play,
        Reverse,
        PlayInstant,
        ReverseInstant
    }

    public enum LoopMode
    {
        Disabled,
        PlayReverseRepeat,
        PlayRepeat,
    }

    public enum PlaybackState
    {
        Idle,
        Playing,
        Reverse,
        Ready
    }

    public enum TweenPropertyType
    {
        None,
        Position,
        Scale,
        Rotation,
        Size,
        Pivot,
        CanvasGroup,
        Custom
    }

    public enum TweenEventType
    {
        Start,
        Update,
        Completed
    }
}