using System;
using Microsoft.Xna.Framework;

namespace TheMythalProphecy.Game.Entities.Components
{
    /// <summary>
    /// Component that manages visual flash effect for battle targeting
    /// Uses sine wave pattern for smooth JRPG-style alpha fading
    /// </summary>
    public class TargetFlashComponent : IComponent
    {
        public Entity Owner { get; set; }
        public bool Enabled { get; set; }

        private float _flashTimer;
        private readonly float _flashCycleTime;
        private readonly float _minAlpha;
        private readonly float _maxAlpha;

        /// <summary>
        /// Current alpha value to apply to the sprite (0.0 to 1.0)
        /// </summary>
        public float CurrentAlpha { get; private set; }

        /// <summary>
        /// Creates a new TargetFlashComponent with customizable flash parameters
        /// </summary>
        /// <param name="cycleTime">Time in seconds for one complete fade in/out cycle (default: 0.6s)</param>
        /// <param name="minAlpha">Minimum alpha value (default: 0.4, never fully invisible)</param>
        /// <param name="maxAlpha">Maximum alpha value (default: 1.0, fully opaque)</param>
        public TargetFlashComponent(float cycleTime = 0.6f, float minAlpha = 0.4f, float maxAlpha = 1.0f)
        {
            _flashCycleTime = cycleTime;
            _minAlpha = minAlpha;
            _maxAlpha = maxAlpha;
            _flashTimer = 0f;
            CurrentAlpha = maxAlpha;
            Enabled = true;
        }

        public void Initialize()
        {
            Reset();
        }

        /// <summary>
        /// Update the flash effect, calculating sine wave alpha
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!Enabled)
            {
                CurrentAlpha = 1.0f;
                return;
            }

            // Increment timer
            _flashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate sine wave position (0 to 2π over cycle time)
            float sineInput = (_flashTimer / _flashCycleTime) * MathF.PI * 2f;

            // Sine wave oscillates between -1 and 1, we want 0 to 1
            float sineValue = (MathF.Sin(sineInput) + 1f) / 2f;

            // Map sine value to alpha range
            CurrentAlpha = _minAlpha + (_maxAlpha - _minAlpha) * sineValue;
        }

        /// <summary>
        /// Reset flash timer to synchronize multiple targets
        /// </summary>
        public void Reset()
        {
            _flashTimer = 0f;
            CurrentAlpha = _maxAlpha;
        }
    }
}
