using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheMythalProphecy.Game.Systems.Animation;

namespace TheMythalProphecy.Game.Entities.Components
{
    /// <summary>
    /// Animation states for characters
    /// </summary>
    public enum AnimationState
    {
        Idle,
        Walk,
        Run,
        Attack,
        Casting,
        Hurt,
        Death,
        Victory,
        Defend,
        Dodge,
        Heal
    }

    /// <summary>
    /// Component that manages entity animations with state-based playback
    /// </summary>
    public class AnimationComponent : IComponent
    {
        private readonly Dictionary<AnimationState, Animation> _animations;
        private AnimationState _currentState;
        private AnimationState _previousState;
        private Animation _currentAnimation;
        private bool _stateChanged;

        public Entity Owner { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Current animation state
        /// </summary>
        public AnimationState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _previousState = _currentState;
                    _currentState = value;
                    _stateChanged = true;
                }
            }
        }

        /// <summary>
        /// Currently playing animation
        /// </summary>
        public Animation CurrentAnimation => _currentAnimation;

        /// <summary>
        /// Whether to automatically return to Idle state after non-looping animations complete
        /// </summary>
        public bool AutoReturnToIdle { get; set; }

        /// <summary>
        /// Event fired when animation state changes
        /// </summary>
        public event Action<AnimationState, AnimationState> OnStateChanged;

        public AnimationComponent()
        {
            _animations = new Dictionary<AnimationState, Animation>();
            _currentState = AnimationState.Idle;
            _previousState = AnimationState.Idle;
            Enabled = true;
            AutoReturnToIdle = true;
        }

        public void Initialize()
        {
            // Set initial animation if available
            if (_animations.TryGetValue(_currentState, out var animation))
            {
                _currentAnimation = animation;
                _currentAnimation.Play();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;

            // Handle state changes
            if (_stateChanged)
            {
                HandleStateChange();
                _stateChanged = false;
            }

            // Update current animation
            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime);

                // Auto-return to idle if animation completed
                if (AutoReturnToIdle && _currentAnimation.IsComplete && _currentState != AnimationState.Idle)
                {
                    CurrentState = AnimationState.Idle;
                }
            }
        }

        /// <summary>
        /// Add an animation for a specific state
        /// </summary>
        public void AddAnimation(AnimationState state, AnimationDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            var animation = new Animation(definition);
            _animations[state] = animation;

            // If this is the current state and we don't have an animation yet, use it
            if (state == _currentState && _currentAnimation == null)
            {
                _currentAnimation = animation;
                _currentAnimation.Play();
            }
        }

        /// <summary>
        /// Add an existing animation for a specific state
        /// </summary>
        public void AddAnimation(AnimationState state, Animation animation)
        {
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            _animations[state] = animation;

            // If this is the current state and we don't have an animation yet, use it
            if (state == _currentState && _currentAnimation == null)
            {
                _currentAnimation = animation;
                _currentAnimation.Play();
            }
        }

        /// <summary>
        /// Check if an animation exists for a state
        /// </summary>
        public bool HasAnimation(AnimationState state)
        {
            return _animations.ContainsKey(state);
        }

        /// <summary>
        /// Get animation for a specific state
        /// </summary>
        public Animation GetAnimation(AnimationState state)
        {
            return _animations.TryGetValue(state, out var animation) ? animation : null;
        }

        /// <summary>
        /// Get the current frame rectangle from the active animation
        /// </summary>
        public Rectangle GetCurrentFrameRectangle()
        {
            return _currentAnimation?.GetCurrentFrameRectangle() ?? Rectangle.Empty;
        }

        /// <summary>
        /// Get the sprite sheet texture from the active animation
        /// </summary>
        public Texture2D GetCurrentTexture()
        {
            return _currentAnimation?.Texture;
        }

        /// <summary>
        /// Play a one-shot animation and optionally return to a state afterward
        /// </summary>
        public void PlayOneShotAnimation(AnimationState state, AnimationState? returnState = null)
        {
            var previousAutoReturn = AutoReturnToIdle;
            AutoReturnToIdle = false;

            CurrentState = state;

            if (_currentAnimation != null)
            {
                _currentAnimation.OnComplete += () =>
                {
                    AutoReturnToIdle = previousAutoReturn;
                    CurrentState = returnState ?? AnimationState.Idle;
                };
            }
        }

        private void HandleStateChange()
        {
            // Try to get animation for new state
            if (_animations.TryGetValue(_currentState, out var newAnimation))
            {
                // Stop previous animation
                _currentAnimation?.Stop();

                // Set and play new animation
                _currentAnimation = newAnimation;
                _currentAnimation.Play();

                // Fire state changed event
                OnStateChanged?.Invoke(_previousState, _currentState);
            }
            else
            {
                // No animation for this state, try to fall back to Idle
                if (_currentState != AnimationState.Idle && _animations.ContainsKey(AnimationState.Idle))
                {
                    _currentState = AnimationState.Idle;
                    _stateChanged = true; // Will trigger another state change on next update
                }
            }
        }
    }
}
