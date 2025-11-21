using System;
using System.Collections.Generic;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Service for managing dynamic gameplay events.
    /// </summary>
    public interface IEventDirector
    {
        /// <summary>
        /// Event fired when a dynamic event is triggered.
        /// </summary>
        event Action<DynamicEvent> OnEventTriggered;

        /// <summary>
        /// Event fired when an event completes.
        /// </summary>
        event Action<string, EventResult> OnEventCompleted;

        /// <summary>
        /// Get all active events.
        /// </summary>
        IReadOnlyList<DynamicEvent> ActiveEvents { get; }

        /// <summary>
        /// Trigger a specific event.
        /// </summary>
        /// <param name="eventType">Type of event to trigger</param>
        /// <param name="region">Event region</param>
        /// <param name="data">Optional event data</param>
        /// <returns>Triggered event or null</returns>
        DynamicEvent TriggerEvent(EventType eventType, string region, object data = null);

        /// <summary>
        /// Complete an event.
        /// </summary>
        /// <param name="eventId">Event ID to complete</param>
        /// <param name="result">Event result</param>
        void CompleteEvent(string eventId, EventResult result);

        /// <summary>
        /// Get event by ID.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Event or null</returns>
        DynamicEvent GetEvent(string eventId);

        /// <summary>
        /// Schedule an event to trigger later.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="delay">Delay in seconds</param>
        /// <param name="region">Event region</param>
        /// <param name="data">Optional event data</param>
        void ScheduleEvent(EventType eventType, float delay, string region, object data = null);

        /// <summary>
        /// Update event director (called each frame).
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void Update(float deltaTime);

        /// <summary>
        /// Initialize event director.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Types of dynamic events.
    /// </summary>
    public enum EventType
    {
        SmugglerSighting,
        PirateRaid,
        ConvoyMovement,
        DistressSignal,
        WeatherEvent,
        IntelReport,
        BorderViolation
    }

    /// <summary>
    /// Event outcome results.
    /// </summary>
    public enum EventResult
    {
        Success,
        Failure,
        Partial,
        Aborted
    }

    /// <summary>
    /// Dynamic event data.
    /// </summary>
    [Serializable]
    public class DynamicEvent
    {
        public string Id;
        public EventType Type;
        public string Region;
        public DateTime StartTime;
        public float Duration;
        public EventPriority Priority;
        public Dictionary<string, object> Data;
        public bool IsActive;

        public DynamicEvent()
        {
            Id = System.Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
            Data = new Dictionary<string, object>();
            IsActive = true;
        }
    }

    /// <summary>
    /// Event priority levels.
    /// </summary>
    public enum EventPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}