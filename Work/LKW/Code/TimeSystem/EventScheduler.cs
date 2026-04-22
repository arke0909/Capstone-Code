using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace Code.TimeSystem
{
    public class TimeEvent
    {
        public int id;
        public float triggerTime;
        public Action action;
        public bool cancelled;
        public float repeatInterval; 
    }
    

    public class EventScheduler
    {
        private int nextId = 0;

        private PriorityQueue<float, TimeEvent> queue = new PriorityQueue<float, TimeEvent>();

        private Dictionary<int, TimeEvent> lookup
            = new Dictionary<int, TimeEvent>();

        public int AddEvent(float currentTime, float delay, Action action)
        {
            var e = new TimeEvent
            {
                id = nextId++,
                triggerTime = currentTime + delay,
                action = action
            };

            queue.Enqueue(e.triggerTime, e);
            lookup[e.id] = e;
            return e.id;
        }

        public int AddRepeat(float currentTime, float interval, Action action)
        {
            var e = new TimeEvent
            {
                id = nextId++,
                triggerTime = currentTime + interval,
                action = action,
                repeatInterval = interval
            };

            queue.Enqueue(e.triggerTime, e);
            lookup[e.id] = e;
            return e.id;
        }

        public void Cancel(int id)
        {
            if (lookup.TryGetValue(id, out var e))
                e.cancelled = true;
        }

        public void Update(float currentTime)
        {
            while (queue.Count > 0)
            {
                TimeEvent e = queue.Peek().Value;

                if (currentTime < e.triggerTime)
                    break;

                queue.Dequeue();

                if (!e.cancelled)
                    e.action?.Invoke();

                if (e.repeatInterval > 0f && !e.cancelled)
                {
                    e.triggerTime += e.repeatInterval;
                    queue.Enqueue(e.triggerTime, e);
                }
                else
                {
                    lookup.Remove(e.id);
                }
            }
        }
    }
}