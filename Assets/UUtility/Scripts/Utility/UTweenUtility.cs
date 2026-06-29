using System;
using UnityEngine;

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core.Easing;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tween"></param>
        /// <returns>Returns true if tween is killed</returns>
        public static bool KillTween(this Tween tween)
        {
            if (tween != null)
            {
                tween.Kill();
                tween = null;
                return true;
            }

            return false;
        }

        public static float Evaluate(this float time, Ease ease)
        {
            return EaseManager.Evaluate(ease, null, time, 1, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod); ;
        }

        public static float EvaluateCurve(this float time, Ease ease, Ease endEase = Ease.Unset)
        {
            float value = 0;
            if (time < 0.5f)
                value = EaseManager.Evaluate(ease, null, time * 2, 1, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
            else
                value = EaseManager.Evaluate(endEase == Ease.Unset? ease : endEase, null, RangedMapUnClamp(time, 0.5f, 1f, 1f, 0f), 1, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
            
            return value;
        }

        public static Tween FadeCanvasGroup(this CanvasGroup canvasGroup, bool state, float duration = 0.3f, Action onComplete = null)
            => canvasGroup.FadeCanvasGroup(state ? 1 : 0, duration: duration, onComplete: onComplete);

        public static Tween FadeCanvasGroup(this CanvasGroup canvasGroup, float alpha, float blockRaycastThreshold = 0.5f, float duration = 0.3f, Action onComplete = null)
        {
            Tween tween = canvasGroup.DOFade(alpha, duration)
               .OnStart(() => canvasGroup.CanvasGroupState(false))
               .OnComplete(() =>
               {
                   bool blockRaycast = alpha > blockRaycastThreshold;
                   if (blockRaycast)
                       canvasGroup.CanvasGroupState(true);

                   onComplete.Invoke();
               });

            return tween;
        }

        private static void CanvasGroupState(this CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.blocksRaycasts = state;
            //canvasGroup.interactable = state;
        }

        public static TweenerCore<Vector2, Vector2, VectorOptions> DOSizeDeltaX(this RectTransform target, float endValue, float duration, bool snapping = false)
            => target.DOSizeDelta(new Vector2(endValue, target.sizeDelta.y), duration, snapping);

        public static TweenerCore<Vector2, Vector2, VectorOptions> DOSizeDeltaY(this RectTransform target, float endValue, float duration, bool snapping = false)
            => target.DOSizeDelta(new Vector2(target.sizeDelta.x, endValue), duration, snapping);
    }
}