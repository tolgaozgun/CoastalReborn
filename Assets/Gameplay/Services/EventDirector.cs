using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Interfaces;
using EventType = Gameplay.Interfaces.EventType;

namespace Gameplay.Services
{
    /// <summary>
    /// Stub implementation of the event director service.
    /// </summary>
    public class EventDirector : MonoBehaviour, IEventDirector
    {
        private List<DynamicEvent> activeEvents = new List<DynamicEvent>();
        private List<ScheduledEvent> scheduledEvents = new List<ScheduledEvent>();

        public IReadOnlyList<DynamicEvent> ActiveEvents => activeEvents.AsReadOnly();

        public event Action<DynamicEvent> OnEventTriggered;
        public event Action<string, EventResult> OnEventCompleted;

        public void Initialize()
        {
            // Initialize event director
            Debug.Log("Event Director initialized");
        }

        public DynamicEvent TriggerEvent(EventType eventType, string region, object data = null)
        {
            DynamicEvent newEvent = new DynamicEvent
            {
                Type = eventType,
                Region = region,
                Data = new Dictionary<string, object>(),
                Duration = 300f, // 5 minutes default
                Priority = EventPriority.Medium
            };

            if (data != null)
            {
                // TODO: Process event data
            }

            activeEvents.Add(newEvent);
            OnEventTriggered?.Invoke(newEvent);

            Debug.Log($"Event triggered: {eventType} in {region}");
            return newEvent;
        }

        public void CompleteEvent(string eventId, EventResult result)
        {
            DynamicEvent eventToComplete = activeEvents.Find(e => e.Id == eventId);
            if (eventToComplete != null)
            {
                eventToComplete.IsActive = false;
                activeEvents.Remove(eventToComplete);
                OnEventCompleted?.Invoke(eventId, result);
                Debug.Log($"Event completed: {eventId} with result: {result}");
            }
        }

        public DynamicEvent GetEvent(string eventId)
        {
            return activeEvents.Find(e => e.Id == eventId);
        }

        public void ScheduleEvent(EventType eventType, float delay, string region, object data = null)
        {
            scheduledEvents.Add(new ScheduledEvent
            {
                EventType = eventType,
                Region = region,
                Data = data,
                TriggerTime = Time.time + delay
            });

            Debug.Log($"Event scheduled: {eventType} in {region} at {Time.time + delay}");
        }

        // Interface implementation - called by external systems
        void IEventDirector.Update(float deltaTime)
        {
            UpdateInternal(deltaTime);
        }

        // Unity's Update method - calls internal update
        private void Update()
        {
            UpdateInternal(Time.deltaTime);
        }

        // Internal update logic
        private void UpdateInternal(float deltaTime)
        {
            // Update active events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                DynamicEvent activeEvent = activeEvents[i];
                activeEvent.Duration -= deltaTime;

                if (activeEvent.Duration <= 0f)
                {
                    CompleteEvent(activeEvent.Id, EventResult.Aborted);
                }
            }

            // Check scheduled events
            for (int i = scheduledEvents.Count - 1; i >= 0; i--)
            {
                ScheduledEvent scheduledEvent = scheduledEvents[i];
                if (Time.time >= scheduledEvent.TriggerTime)
                {
                    TriggerEvent(scheduledEvent.EventType, scheduledEvent.Region, scheduledEvent.Data);
                    scheduledEvents.RemoveAt(i);
                }
            }
        }

        private class ScheduledEvent
        {
            public EventType EventType;
            public string Region;
            public object Data;
            public float TriggerTime;
        }
    }
}