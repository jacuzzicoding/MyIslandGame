using Microsoft.Xna.Framework;

namespace MyIslandGame
{
    public class TimeManager
    {
        public Color AmbientLightColor { get; private set; }

        public TimeManager()
        {
            // Initialize with default ambient light color
            AmbientLightColor = Color.White;
        }

        public void Update(GameTime gameTime)
        {
            // Update ambient light color based on time of day or other logic
            // For simplicity, let's cycle through colors over time
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            AmbientLightColor = new Color(
                (float)Math.Sin(time) * 0.5f + 0.5f,
                (float)Math.Sin(time + MathHelper.PiOver2) * 0.5f + 0.5f,
                (float)Math.Sin(time + MathHelper.Pi) * 0.5f + 0.5f);
        }
    }
}
