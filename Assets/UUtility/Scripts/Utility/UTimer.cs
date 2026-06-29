using System;
using UnityEngine;

namespace UTool.Utility
{
    public class UTimer
    {
        private bool autoStart = false;
        private float startDelay = 0;

        private float timer;

        private bool canTick = false;
        private bool isTickDelayed = true;

        private float tickDelayTimer;

        public UTimer(bool autoStart = true, float startDelay = 0)
        {
            this.autoStart = autoStart;
            this.startDelay = startDelay;
        }

        public void Start()
        {
            autoStart = false;

            isTickDelayed = true;
            tickDelayTimer = 0;

            Play();
        }

        public void Play()
        {
            canTick = true;
        }

        public void Pause()
        {
            canTick = false;
        }

        public void Stop()
        {
            Pause();
            isTickDelayed = true;

            timer = 0;
        }

        public void TickPerSec(float tickRate, Action<float> onTickUpdate = null, Action onTick = null)
            => Tick(1 / tickRate, onTickUpdate: onTickUpdate, onTick: onTick);

        public void TickPerMin(float tickRate, Action<float> onTickUpdate = null, Action onTick = null)
            => Tick(60 / tickRate, onTickUpdate: onTickUpdate, onTick: onTick);

        private void Tick(float interval, Action<float> onTickUpdate = null, Action onTick = null)
        {
            if (!canTick)
            {
                if (autoStart)
                    Start();

                return;
            }

            if (isTickDelayed)
            {
                tickDelayTimer += Time.deltaTime;

                if (tickDelayTimer >= startDelay)
                    isTickDelayed = false;

                return;
            }

            timer += Time.deltaTime;

            float percent = UUtility.RangedMapClamp(timer, 0, interval, 0, 1);
            onTickUpdate?.Invoke(percent);

            if (timer > interval)
            {
                timer -= interval;
                onTick?.Invoke();
            }
        }
    }
}