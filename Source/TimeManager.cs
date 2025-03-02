using System;
using Microsoft.Xna.Framework;

namespace MyIslandGame
{
    public class TimeManager
    {
        public Color AmbientLightColor { get; private set; }
        private float _totalGameMinutes;
        private readonly float _minutesPerSecond;
        private readonly float _minutesPerDay;
        private bool _isPaused;

        // Enum for time of day
        public enum TimeOfDay
        {
            Day,
            Sunset,
            Night,
            Sunrise
        }

        // Properties for accessing time information
        public float CurrentTimeMinutes => _totalGameMinutes % _minutesPerDay;
        public float MinutesPerSecond => _minutesPerSecond;
        public float MinutesPerDay => _minutesPerDay;
        public int CurrentHour => (int)(CurrentTimeMinutes / 60f) % 24;
        public int CurrentMinute => (int)(CurrentTimeMinutes % 60f);
        public int CurrentDay => (int)(_totalGameMinutes / _minutesPerDay) + 1;
        public float NormalizedTime => CurrentTimeMinutes / _minutesPerDay;
        
        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        // Default constructor
        public TimeManager()
            : this(1440f, 24f, 480f) // 1440 minutes per day, 24 minutes per second, start at 8:00 AM
        {
        }

        // Full constructor
        public TimeManager(float minutesPerDay, float minutesPerSecond, float startTimeMinutes)
        {
            _minutesPerDay = minutesPerDay;
            _minutesPerSecond = minutesPerSecond;
            _totalGameMinutes = startTimeMinutes;
            _isPaused = false;
            AmbientLightColor = Color.White;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isPaused)
            {
                float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                float deltaMinutes = deltaSeconds * _minutesPerSecond;
                
                _totalGameMinutes += deltaMinutes;
            }

            // Update ambient light color based on time of day
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            AmbientLightColor = new Color(
                (float)Math.Sin(time) * 0.5f + 0.5f,
                (float)Math.Sin(time + MathHelper.PiOver2) * 0.5f + 0.5f,
                (float)Math.Sin(time + MathHelper.Pi) * 0.5f + 0.5f);
        }

        // Current time of day phase
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

        // Sun intensity (0.0 to 1.0)
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
                return 1.0f - (normalizedTime - 19f / 24f) / (2f / 24f);
            }
        }

        // Set time to specific hour and minute
        public void SetTime(int hour, int minute)
        {
            hour = MathHelper.Clamp(hour, 0, 23);
            minute = MathHelper.Clamp(minute, 0, 59);
            
            float currentDay = (float)Math.Floor(_totalGameMinutes / _minutesPerDay);
            _totalGameMinutes = (currentDay * _minutesPerDay) + (hour * 60f) + minute;
        }

        // Get formatted time string (HH:MM)
        public string GetTimeString()
        {
            return $"{CurrentHour:D2}:{CurrentMinute:D2}";
        }
    }
}
