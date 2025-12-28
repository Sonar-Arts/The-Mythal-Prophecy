using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Animation
{
    /// <summary>
    /// Represents a frame-based animation with playback control
    /// </summary>
    public class Animation
    {
        private readonly AnimationDefinition _definition;
        private int _currentFrame;
        private float _elapsedTime;
        private bool _isPlaying;
        private bool _isPaused;
        private int _loopCount;

        public string Name => _definition.Name;
        public int CurrentFrame => _currentFrame;
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public bool IsComplete => !_definition.Loop && _currentFrame >= _definition.FrameCount - 1;
        public int LoopCount => _loopCount;
        public Texture2D Texture => _definition.SpriteSheet;

        /// <summary>
        /// Event fired when animation completes (for non-looping animations)
        /// </summary>
        public event Action OnComplete;

        /// <summary>
        /// Event fired when animation loops
        /// </summary>
        public event Action OnLoop;

        /// <summary>
        /// Event fired when a specific frame is reached
        /// </summary>
        public event Action<int> OnFrameChanged;

        public Animation(AnimationDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _currentFrame = 0;
            _elapsedTime = 0f;
            _isPlaying = false;
            _isPaused = false;
            _loopCount = 0;
        }

        /// <summary>
        /// Start playing the animation from the beginning
        /// </summary>
        public void Play()
        {
            _isPlaying = true;
            _isPaused = false;
            _currentFrame = 0;
            _elapsedTime = 0f;
            _loopCount = 0;
        }

        /// <summary>
        /// Resume playing the animation from current position
        /// </summary>
        public void Resume()
        {
            if (_isPaused)
            {
                _isPaused = false;
                _isPlaying = true;
            }
        }

        /// <summary>
        /// Pause the animation
        /// </summary>
        public void Pause()
        {
            if (_isPlaying)
            {
                _isPaused = true;
                _isPlaying = false;
            }
        }

        /// <summary>
        /// Stop the animation and reset to first frame
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            _currentFrame = 0;
            _elapsedTime = 0f;
            _loopCount = 0;
        }

        /// <summary>
        /// Update animation state
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_isPlaying || _isPaused)
                return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate frame duration
            float frameDuration = _definition.FrameDuration;

            // Check if we need to advance to next frame
            while (_elapsedTime >= frameDuration)
            {
                _elapsedTime -= frameDuration;
                int previousFrame = _currentFrame;
                _currentFrame++;

                // Handle animation completion/looping
                if (_currentFrame >= _definition.FrameCount)
                {
                    if (_definition.Loop)
                    {
                        _currentFrame = 0;
                        _loopCount++;
                        OnLoop?.Invoke();
                    }
                    else
                    {
                        _currentFrame = _definition.FrameCount - 1;
                        _isPlaying = false;
                        OnComplete?.Invoke();
                    }
                }

                // Fire frame changed event
                if (_currentFrame != previousFrame)
                {
                    OnFrameChanged?.Invoke(_currentFrame);
                }
            }
        }

        /// <summary>
        /// Get the source rectangle for the current frame
        /// </summary>
        public Rectangle GetCurrentFrameRectangle()
        {
            return _definition.GetFrameRectangle(_currentFrame);
        }

        /// <summary>
        /// Get normalized progress (0.0 to 1.0) through the animation
        /// </summary>
        public float GetProgress()
        {
            if (_definition.FrameCount <= 1)
                return 1.0f;

            return (float)_currentFrame / (_definition.FrameCount - 1);
        }

        /// <summary>
        /// Reset animation to first frame
        /// </summary>
        public void Reset()
        {
            _currentFrame = 0;
            _elapsedTime = 0f;
            _loopCount = 0;
        }

        /// <summary>
        /// Set animation to a specific frame
        /// </summary>
        public void SetFrame(int frame)
        {
            if (frame < 0 || frame >= _definition.FrameCount)
                throw new ArgumentOutOfRangeException(nameof(frame));

            _currentFrame = frame;
            _elapsedTime = 0f;
        }
    }
}
