using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using TheMythalProphecy.Game.Core;

namespace TheMythalProphecy.Game.Systems.Rendering
{
    /// <summary>
    /// Static manager for loading and caching battle backgrounds
    /// </summary>
    public static class BattleBackgroundManager
    {
        /// <summary>
        /// Available battleground themes
        /// </summary>
        public enum BattlegroundTheme
        {
            Ruins,
            Castle,
            Jungle,
            Graveyard
        }

        /// <summary>
        /// Color variants for battlegrounds
        /// </summary>
        public enum Variant
        {
            Bright,
            Pale
        }

        private static readonly Dictionary<string, BattleBackground> _backgrounds = new Dictionary<string, BattleBackground>();
        private static bool _initialized = false;

        /// <summary>
        /// Initializes the battle background manager
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            Console.WriteLine("[BattleBackgroundManager] Initialized");
        }

        /// <summary>
        /// Loads a battleground by theme and variant
        /// </summary>
        public static BattleBackground LoadBattleground(BattlegroundTheme theme, Variant variant = Variant.Bright)
        {
            string key = $"{theme}_{variant}";

            // Return cached background if already loaded
            if (_backgrounds.TryGetValue(key, out var cached))
                return cached;

            // Create new background based on theme
            BattleBackground background = theme switch
            {
                BattlegroundTheme.Ruins => CreateRuinsBattleground(variant),
                BattlegroundTheme.Castle => CreateCastleBattleground(variant),
                BattlegroundTheme.Jungle => CreateJungleBattleground(variant),
                BattlegroundTheme.Graveyard => CreateGraveyardBattleground(variant),
                _ => throw new ArgumentException($"Unknown battleground theme: {theme}")
            };

            // Cache and return
            _backgrounds[key] = background;
            Console.WriteLine($"[BattleBackgroundManager] Loaded battleground: {key}");
            return background;
        }

        /// <summary>
        /// Gets a previously loaded background by key
        /// </summary>
        public static BattleBackground Get(string key)
        {
            return _backgrounds.TryGetValue(key, out var background) ? background : null;
        }

        /// <summary>
        /// Clears all cached backgrounds
        /// </summary>
        public static void ClearCache()
        {
            foreach (var bg in _backgrounds.Values)
            {
                bg?.Dispose();
            }
            _backgrounds.Clear();
            Console.WriteLine("[BattleBackgroundManager] Cache cleared");
        }

        #region Ruins Battleground

        private static BattleBackground CreateRuinsBattleground(Variant variant)
        {
            var bg = new BattleBackground($"Ruins_{variant}");
            var basePath = Path.Combine("Game", "Art", "Backgrounds", "Battlegrounds", "Ruins", variant.ToString());

            // Layer 0: Sky (static background)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "0_sky.png")),
                ScrollFactor = 0.0f,
                RenderLayer = RenderLayer.Background,
                Depth = 0.0f
            });

            // Layer 1: Hills & Trees (far parallax)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "1_hills&trees.png")),
                ScrollFactor = 0.15f,
                RenderLayer = RenderLayer.BackgroundFar,
                Depth = 0.0f
            });

            // Layer 2: Ruins Background (mid parallax)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "2_ruins_bg.png")),
                ScrollFactor = 0.3f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.0f
            });

            // Layer 3: Ruins (mid-near parallax)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "3_ruins.png")),
                ScrollFactor = 0.5f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.01f
            });

            // Layer 4: Ruins2 (near parallax)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "4_ruins2.png")),
                ScrollFactor = 0.7f,
                RenderLayer = RenderLayer.BackgroundNear,
                Depth = 0.0f
            });

            // Layer 5: Stones & Grass (ground level)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "5_stones&grass.png")),
                ScrollFactor = 0.9f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.0f
            });

            // Layer 6: Statue (foreground decoration)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "6_statue.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Foreground,
                Depth = 0.0f
            });

            // Layer 7: Composite/Ground
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "7_Battleground1.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.01f
            });

            return bg;
        }

        #endregion

        #region Castle Battleground

        private static BattleBackground CreateCastleBattleground(Variant variant)
        {
            var bg = new BattleBackground($"Castle_{variant}");
            var basePath = Path.Combine("Game", "Art", "Backgrounds", "Battlegrounds", "Castle", variant.ToString());

            // Layer 0: Background
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "0_bg.png")),
                ScrollFactor = 0.0f,
                RenderLayer = RenderLayer.Background,
                Depth = 0.0f
            });

            // Layer 1: Mountains
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "1_mountaims.png")),
                ScrollFactor = 0.2f,
                RenderLayer = RenderLayer.BackgroundFar,
                Depth = 0.0f
            });

            // Layer 2: Dragon
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "2_dragon.png")),
                ScrollFactor = 0.4f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.0f
            });

            // Layer 3: Wall & Windows
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "3_wall@windows.png")),
                ScrollFactor = 0.6f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.01f
            });

            // Layer 4: Columns & Flags
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "4_columns&falgs.png")),
                ScrollFactor = 0.8f,
                RenderLayer = RenderLayer.BackgroundNear,
                Depth = 0.0f
            });

            // Layer 5: Candelabra
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "5_candeliar.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.BackgroundNear,
                Depth = 0.01f
            });

            // Layer 6: Floor
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "6_floor.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.0f
            });

            // Layer 7: Composite
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "7_Battleground2.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.01f
            });

            return bg;
        }

        #endregion

        #region Jungle Battleground

        private static BattleBackground CreateJungleBattleground(Variant variant)
        {
            var bg = new BattleBackground($"Jungle_{variant}");
            var basePath = Path.Combine("Game", "Art", "Backgrounds", "Battlegrounds", "Jungle", variant.ToString());

            // Layer 0: Sky
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "0_sky.png")),
                ScrollFactor = 0.0f,
                RenderLayer = RenderLayer.Background,
                Depth = 0.0f
            });

            // Layer 1: Jungle Background
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "1_jungle_bg.png")),
                ScrollFactor = 0.15f,
                RenderLayer = RenderLayer.BackgroundFar,
                Depth = 0.0f
            });

            // Layer 2: Trees & Bushes
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "2_trees&bushes.png")),
                ScrollFactor = 0.3f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.0f
            });

            // Layer 3: Tree Face
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "3_tree_face.png")),
                ScrollFactor = 0.5f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.01f
            });

            // Layer 4: Lianas
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "4_lianas.png")),
                ScrollFactor = 0.7f,
                RenderLayer = RenderLayer.BackgroundNear,
                Depth = 0.0f
            });

            // Layer 5: Grass & Road
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "5_grass&road.png")),
                ScrollFactor = 0.9f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.0f
            });

            // Layer 6: Grasses (foreground)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "6_grasses.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Foreground,
                Depth = 0.0f
            });

            // Layer 7: Fireflies (animated layer)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "7_fireflys.png")),
                ScrollFactor = 0.5f,
                RenderLayer = RenderLayer.Effects,
                Depth = 0.0f
            });

            // Layer 8: Composite
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "8_Battleground3.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.01f
            });

            return bg;
        }

        #endregion

        #region Graveyard Battleground

        private static BattleBackground CreateGraveyardBattleground(Variant variant)
        {
            var bg = new BattleBackground($"Graveyard_{variant}");
            var basePath = Path.Combine("Game", "Art", "Backgrounds", "Battlegrounds", "Graveyard", variant.ToString());

            // Layer 0: Sky
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "0_sky.png")),
                ScrollFactor = 0.0f,
                RenderLayer = RenderLayer.Background,
                Depth = 0.0f
            });

            // Layer 1: Back Trees
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "1_back_trees.png")),
                ScrollFactor = 0.2f,
                RenderLayer = RenderLayer.BackgroundFar,
                Depth = 0.0f
            });

            // Layer 2: Crypt
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "2_crypt.png")),
                ScrollFactor = 0.4f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.0f
            });

            // Layer 3: Wall
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "3_wall.png")),
                ScrollFactor = 0.6f,
                RenderLayer = RenderLayer.BackgroundMid,
                Depth = 0.01f
            });

            // Layer 4: Tree
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "4_tree.png")),
                ScrollFactor = 0.8f,
                RenderLayer = RenderLayer.BackgroundNear,
                Depth = 0.0f
            });

            // Layer 5: Graves
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "5_graves.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.0f
            });

            // Layer 6: Ground
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "6_ground.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.005f
            });

            // Layer 7: Bones (foreground)
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "7_bones.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Foreground,
                Depth = 0.0f
            });

            // Layer 8: Composite
            bg.AddLayer(new ParallaxLayer
            {
                Texture = LoadTexture(Path.Combine(basePath, "8_Battleground4.png")),
                ScrollFactor = 1.0f,
                RenderLayer = RenderLayer.Ground,
                Depth = 0.01f
            });

            return bg;
        }

        #endregion

        #region Texture Loading

        /// <summary>
        /// Loads a texture from file using FromStream pattern
        /// </summary>
        private static Texture2D LoadTexture(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var texture = Texture2D.FromStream(GameServices.GraphicsDevice, stream);
                return texture;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"[BattleBackgroundManager] ERROR: Texture not found: {path}");
                return CreateFallbackTexture();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleBackgroundManager] ERROR loading {path}: {ex.Message}");
                return CreateFallbackTexture();
            }
        }

        /// <summary>
        /// Creates a 1x1 magenta texture for debugging missing textures
        /// </summary>
        private static Texture2D CreateFallbackTexture()
        {
            var texture = new Texture2D(GameServices.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Microsoft.Xna.Framework.Color.Magenta });
            return texture;
        }

        #endregion
    }
}
