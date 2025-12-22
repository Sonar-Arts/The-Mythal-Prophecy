using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheMythalProphecy.Game.Core;

namespace TheMythalProphecy.Game.Systems.Animation
{
    /// <summary>
    /// Centralized library for all character animation definitions
    /// Loads animations from the Game/Art/Characters/Animations directory
    /// </summary>
    public static class AnimationLibrary
    {
        /// <summary>
        /// Animation categories for organization
        /// </summary>
        public enum AnimationCategory
        {
            Combat,
            Movement,
            Ranged,
            Aerial,
            Special
        }

        private static bool _initialized = false;
        private static readonly Dictionary<AnimationCategory, List<string>> _animationsByCategory = new Dictionary<AnimationCategory, List<string>>();

        /// <summary>
        /// Initializes the animation library with example animations
        /// </summary>
        public static void Initialize(AnimationManager animationManager)
        {
            if (_initialized)
            {
                Console.WriteLine("[AnimationLibrary] Already initialized");
                return;
            }

            if (animationManager == null)
                throw new ArgumentNullException(nameof(animationManager));

            try
            {
                // Initialize category tracking
                foreach (AnimationCategory category in Enum.GetValues(typeof(AnimationCategory)))
                {
                    _animationsByCategory[category] = new List<string>();
                }

                // Register example animations (10 as specified)
                RegisterExampleAnimations(animationManager);

                _initialized = true;
                Console.WriteLine("[AnimationLibrary] Initialized with example animations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationLibrary] ERROR during initialization: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Registers the 10 example animation definitions
        /// NOTE: Frame counts are estimates and should be measured from actual sprite sheets
        /// </summary>
        private static void RegisterExampleAnimations(AnimationManager animationManager)
        {
            // Combat animations
            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_attack", "Combat", "Attack.png",
                frameCount: 8, frameDuration: 0.08f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_cast", "Combat", "Casting Spell.png",
                frameCount: 10, frameDuration: 0.1f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_hurt", "Combat", "Taking_Damage.png",
                frameCount: 4, frameDuration: 0.08f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_death", "Combat", "Death.png",
                frameCount: 12, frameDuration: 0.1f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_dodge", "Combat", "Dodge.png",
                frameCount: 6, frameDuration: 0.06f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_use_item", "Combat", "Drinking_Potion.png",
                frameCount: 8, frameDuration: 0.1f, loop: false);

            RegisterAnimation(animationManager, AnimationCategory.Combat,
                "character_defend", "Combat", "Defensive_Stance.png",
                frameCount: 5, frameDuration: 0.1f, loop: false);

            // Movement animations
            RegisterAnimation(animationManager, AnimationCategory.Movement,
                "character_walk", "Movement", "Walking.png",
                frameCount: 6, frameDuration: 0.12f, loop: true);

            RegisterAnimation(animationManager, AnimationCategory.Movement,
                "character_run", "Movement", "Running.png",
                frameCount: 8, frameDuration: 0.08f, loop: true);

            // Idle animation (reusing walking frames with slower timing)
            RegisterAnimation(animationManager, AnimationCategory.Movement,
                "character_idle", "Movement", "Walking.png",
                frameCount: 4, frameDuration: 0.2f, loop: true);

            Console.WriteLine($"[AnimationLibrary] Registered 10 example animations");
        }

        /// <summary>
        /// Registers a single horizontal strip animation
        /// </summary>
        private static void RegisterAnimation(
            AnimationManager animationManager,
            AnimationCategory category,
            string name,
            string subfolder,
            string filename,
            int frameCount,
            float frameDuration,
            bool loop)
        {
            try
            {
                // Build file path
                var path = Path.Combine("Game", "Art", "Characters", "Animations", subfolder, filename);

                // Load texture using FromStream pattern
                var texture = LoadTexture(path);

                // Create animation definition using static factory method
                var definition = AnimationDefinition.CreateHorizontalStrip(
                    name,
                    texture,
                    frameCount,
                    frameDuration,
                    loop
                );

                // Register with animation manager
                animationManager.RegisterAnimation(definition);

                // Track in category
                _animationsByCategory[category].Add(name);

                Console.WriteLine($"[AnimationLibrary] Registered: {name} ({frameCount} frames, {frameDuration}s/frame, loop={loop})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationLibrary] ERROR registering {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a texture from file using FromStream pattern (bypasses Content Pipeline)
        /// </summary>
        private static Texture2D LoadTexture(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[AnimationLibrary] WARNING: File not found: {path}");
                    return CreateFallbackTexture();
                }

                using var stream = File.OpenRead(path);
                var texture = Texture2D.FromStream(GameServices.GraphicsDevice, stream);
                return texture;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"[AnimationLibrary] ERROR: Texture not found: {path}");
                return CreateFallbackTexture();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationLibrary] ERROR loading {path}: {ex.Message}");
                return CreateFallbackTexture();
            }
        }

        /// <summary>
        /// Creates a 1x1 magenta texture for debugging missing textures
        /// </summary>
        private static Texture2D CreateFallbackTexture()
        {
            var texture = new Texture2D(GameServices.GraphicsDevice, 192, 192);
            var data = new Microsoft.Xna.Framework.Color[192 * 192];
            for (int i = 0; i < data.Length; i++)
                data[i] = Microsoft.Xna.Framework.Color.Magenta;
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Gets all animation names in a specific category
        /// </summary>
        public static IEnumerable<string> GetAnimationsByCategory(AnimationCategory category)
        {
            if (!_initialized)
                return Enumerable.Empty<string>();

            return _animationsByCategory.TryGetValue(category, out var animations)
                ? animations
                : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the total number of registered animations
        /// </summary>
        public static int GetAnimationCount()
        {
            if (!_initialized)
                return 0;

            return _animationsByCategory.Values.Sum(list => list.Count);
        }

        /// <summary>
        /// Checks if the library has been initialized
        /// </summary>
        public static bool IsInitialized => _initialized;
    }
}
