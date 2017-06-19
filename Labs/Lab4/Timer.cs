﻿using System;

namespace Labs.Lab4
{
    public class Timer
    {
        DateTime mLastTime;

        public Timer()
        {}

        public void Start()
        {
            mLastTime = DateTime.Now;
        }

        public float GetElapsedSeconds()
        {
            //return 3.0f / 200.0f;
            DateTime now = DateTime.Now;
            TimeSpan elasped = now - mLastTime;
            mLastTime = now;
            return (float)elasped.Ticks / TimeSpan.TicksPerSecond;
        }
    }
}
