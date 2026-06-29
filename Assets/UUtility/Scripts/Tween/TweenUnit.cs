using System;
using UnityEngine;

using UTool.Utility;
using UTool.Tweening.Helper;

namespace UTool.Tweening
{
    public class TweenUnit : MonoBehaviour
    {
        [SerializeField][BeginGroup] public TransformType transformType;
        [SerializeField][ShowIf(nameof(isRectTransform))] private RectTransform tweenRect;
        [SerializeField][EndGroup][ShowIf(nameof(isTransform))] private Transform tweenTransform;

        [SpaceArea]
        [EditorButton(nameof(PlayTween), activityType: ButtonActivityType.OnPlayMode)]
        [EditorButton(nameof(ReverseTween), activityType: ButtonActivityType.OnPlayMode)]
        [EditorButton(nameof(StopTween), activityType: ButtonActivityType.OnPlayMode)]
        [SpaceArea]

        [SerializeField][BeginGroup][SearchableEnum] private AutoStartTween autoStart = AutoStartTween.Disabled;
        [SerializeField][SearchableEnum] private LoopMode loopMode = LoopMode.Disabled;
        [SerializeField] private int loopCount = -1;
        [SerializeField][EndGroup][Disable] private int loopCounter = 0;

        [SerializeField][IgnoreParent] public TweenProperty tweenProperty;

        private bool isRectTransform => transformType == TransformType.RectTransform;
        private bool isTransform => transformType == TransformType.Transform;

        private Action onCompleteCallback;

        private bool latestRequestState;
        private bool loopCompleted = false;

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
            tweenProperty.OnPropertyChanged = UpdateProperty;
            tweenProperty.OnComplete = PropertyComplete;
        }

        private void Start()
        {
            switch (autoStart)
            {
                case AutoStartTween.Play:
                    PlayTween();
                    break;

                case AutoStartTween.Reverse:
                    ReverseTween();
                    break;
            }
        }

        private void UpdateCurrentValue(bool updateFromValue)
        {
            if (transformType == TransformType.RectTransform)
            {
                if (tweenRect)
                {
                    tweenProperty.SetCurrentValue(tweenRect);
                    if (updateFromValue)
                        tweenProperty.UpdateFromValue();
                }
            }
            else
            {
                if (tweenTransform)
                {
                    tweenProperty.SetCurrentValue(tweenTransform);
                    if (updateFromValue)
                        tweenProperty.UpdateFromValue();
                }
            }
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
            onCompleteCallback?.Invoke();
            onCompleteCallback = null;

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

        public void PlayTween(Action OnComplete) => Show(true, OnComplete);
        public void ReverseTween(Action OnComplete) => Show(false, OnComplete);

        public void PlayTween() => Show(true);
        public void ReverseTween() => Show(false);
        public void Show(bool state, Action OnComplete = null)
        {
            PlaybackState requestedPlayback = state ? PlaybackState.Playing : PlaybackState.Reverse;

            if (requestedPlayback == tweenProperty.playbackState)
                return;

            if (loopCompleted)
            {
                loopCounter = 0;
                loopCompleted = false;
            }

            onCompleteCallback = OnComplete;

            if (state)
                tweenProperty.PlayTween();
            else
                tweenProperty.ReverseTween();

            latestRequestState = state;
        }

        public void ResetTween(bool state)
        {
            tweenProperty.ResetTween(state);
        }

        public void StopTween() => KillTween();
        private void KillTween()
        {
            tweenProperty.KillTween();
        }
    }
}