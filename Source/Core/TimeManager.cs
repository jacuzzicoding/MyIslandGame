using System;
using Microsoft.Xna.Framework;

namespace MyIslandGame.Core
{
    /// <summary>
    /// Manages game time, including day/night cycles.
    /// </summary>
    public class TimeManager
    {
        /// <summary>
        /// Represents a time of day in the game world.
        /// </summary>
        public enum TimeOfDay
        {
            /// <summary>Daytime.</summary>
            Day,
            
            /// <summary>Sunset time.</summary>
            Sunset,
            
            /// <summary>Nighttime.</summary>
            Night,
            
            /// <summary>Sunrise time.</summary>
            Sunrise
        }
        
        private float _totalGameMinutes;
        private readonly float _minutesPerSecond;
        private readonly float _minutesPerDay;
        private bool _isPaused;
        
        /// <summary>
        /// Gets the current time of day in game minutes (0 to MinutesPerDay).
        /// </summary>
        public float CurrentTimeMinutes => _totalGameMinutes % _minutesPerDay;
        
        /// <summary>
        /// Gets the number of in-game minutes per real-time second.
        /// </summary>
        public float MinutesPerSecond => _minutesPerSecond;
        
        /// <summary>
        /// Gets the number of minutes in a full game day.
        /// </summary>
        public float MinutesPerDay => _minutesPerDay;
        
        /// <summary>
        /// Gets the current in-game hour (0-23).
        /// </summary>
        public int CurrentHour => (int)(CurrentTimeMinutes / 60f) % 24;
        
        /// <summary>
        /// Gets the current in-game minute (0-59).
        /// </summary>
        public int CurrentMinute => (int)(CurrentTimeMinutes % 60f);
        
        /// <summary>
        /// Gets the current in-game day number, starting from day 1.
        /// </summary>
        public int CurrentDay => (int)(_totalGameMinutes / _minutesPerDay) + 1;
        
        /// <summary>
        /// Gets the current time progress as a normalized value (0.0 to 1.0).
        /// </summary>
        public float NormalizedTime => CurrentTimeMinutes / _minutesPerDay;
        
        /// <summary>
        /// Gets or sets whether the time is paused.
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }
        
        /// <summary>
        /// Gets the current time of day phase.
        /// </summary>
        public TimeOfDay CurrentTimeOfDay
        {
            get
            {
                float normalizedTime = NormalizedTime;
                
                // Dawn hours (5:00 - 7:00)
                if (normalizedTime >= 5f / 24f && normalizedTime < 7f / 24f)
                {
                    return TimeOfDay.Sunrise;
                }
                // Day hours (7:00 - 19:00)
                else if (normalizedTime >= 7f / 24f && normalizedTime < 19f / 24f)
                {
                    return TimeOfDay.Day;
                }
                // Dusk hours (19:00 - 21:00)
                else if (normalizedTime >= 19f / 24f && normalizedTime < 21f / 24f)
                {
                    return TimeOfDay.Sunset;
                }
                // Night hours (21:00 - 5:00)
                else
                {
                    return TimeOfDay.Night;
                }
            }
        }
        
        /// <summary>
        /// Gets the sun intensity factor (0.0 to 1.0).
        /// </summary>
        public float SunIntensity
        {
            get
            {
                float normalizedTime = NormalizedTime;
                
                // Night (0.0)
                if (normalizedTime < 5f / 24f || normalizedTime >= 21f / 24f)
                {
                    return 0.0f;
                }
                
                // Sunrise transition (0.0 to 1.0)
                if (normalizedTime >= 5f / 24f && normalizedTime < 7f / 24f)
                {
                    return (normalizedTime - 5f / 24f) / (2f / 24f);
                }
                
                // Daytime (1.0)
                if (normalizedTime >= 7f / 24f && normalizedTime < 19f / 24f)
                {
                    return 1.0f;
                }
                
                // Sunset transition (1.0 to 0.0)
                if (normalizedTime >= 19f / 24f && normalizedTime < 21f / 24f)
                {
                    return 1.0f - (normalizedTime - 19f / 24f) / (2f / 24f);
                }
                
                return 0.0f;
            }
        }
        
        /// <summary>
        /// Gets a color representing the ambient light based on the time of day.
        /// </summary>
        public Color AmbientLightColor
        {
            get
            {
                TimeOfDay timeOfDay = CurrentTimeOfDay;
                float sunIntensity = SunIntensity;
                
                switch (timeOfDay)
                {
                    case TimeOfDay.Day:
                        return Color.White;
                        
                    case TimeOfDay.Sunrise:
                        // Sunrise: Blend from dark blue to white with orange tint
                        return Color.Lerp(
                            Color.Lerp(new Color(20, 20, 70), new Color(255, 200, 150), sunIntensity),
                            Color.White,
                            sunIntensity);
                        
                    case TimeOfDay.Sunset:
                        // Sunset: Blend from white to dark blue with orange tint
                        return Color.Lerp(
                            Color.White,
                            Color.Lerp(new Color(255, 150, 100), new Color(20, 20, 70), 1f - sunIntensity),
                            1f - sunIntensity);
                        
                    case TimeOfDay.Night:
                        // Night: Dark blue with slight moonlight
                        return new Color(20, 20, 70);
                        
                    default:
                        return Color.White;
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeManager"/> class.
        /// </summary>
        /// <param name="minutesPerDay">Number of minutes in a game day.</param>
        /// <param name="minutesPerSecond">Number of game minutes per real second.</param>
        /// <param name="startTimeMinutes">Starting time in minutes.</param>
        public TimeManager(float minutesPerDay = 1440f, float minutesPerSecond = 24f, float startTimeMinutes = 480f)
        {
            _minutesPerDay = minutesPerDay;
            _minutesPerSecond = minutesPerSecond;
            _totalGameMinutes = startTimeMinutes; // Start at 8:00 AM by default
            _isPaused = false;
        }
        
        /// <summary>
        /// Updates the time manager.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            if (!_isPaused)
            {
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                float deltaMinutes = deltaSeconds * _minutesPerSecond;
                
                _totalGameMinutes += deltaMinutes;
            }
        }
        
        /// <summary>
        /// Sets the time to a specific hour and minute.
        /// </summary>
        /// <param name="hour">The hour (0-23).</param>
        /// <param name="minute">The minute (0-59).</param>
        public void SetTime(int hour, int minute)
        {
            hour = MathHelper.Clamp(hour, 0, 23);
            minute = MathHelper.Clamp(minute, 0, 59);
            
            float currentDay = (float)Math.Floor(_totalGameMinutes / _minutesPerDay);
            _totalGameMinutes = (currentDay * _minutesPerDay) + (hour * 60f) + minute;
        }
        
        /// <summary>
        /// Gets a formatted time string (HH:MM).
        /// </summary>
        /// <returns>The formatted time string.</returns>
        public string GetTimeString()
        {
            return $"{CurrentHour:D2}:{CurrentMinute:D2}";
        }
        
        /// <summary>
        /// Gets a formatted date and time string (Day X - HH:MM).
        /// </summary>
        /// <returns>The formatted date and time string.</returns>
        public string GetDateTimeString()
        {
            return $"Day {CurrentDay} - {GetTimeString()}";
        }
    }
}
