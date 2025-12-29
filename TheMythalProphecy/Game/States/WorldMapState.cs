using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Data.Mock;
using TheMythalProphecy.Game.Systems.Rendering;

namespace TheMythalProphecy.Game.States;

/// <summary>
/// World map state - main game state showing the world map with HUD
/// Placeholder for testing menu integration
/// </summary>
public class WorldMapState : IGameState
{
    private readonly GameStateManager _stateManager;
    private HUDManager _hud;
    private KeyboardState _previousKeyState;
    private SpriteFont _debugFont;
    private Random _random;
    private int _stepCounter;

    public WorldMapState(GameStateManager stateManager)
    {
        _stateManager = stateManager;
        _random = new Random();
        _stepCounter = 0;
    }

    public void Enter()
    {
        try
        {
            Console.WriteLine("[WorldMapState] Enter() called");

            // Load debug font
            _debugFont = GameServices.Content.Load<SpriteFont>("Fonts/Default");
            Console.WriteLine($"[WorldMapState] Debug font loaded: {_debugFont != null}");

            // Check theme
            var theme = GameServices.UI?.Theme;
            Console.WriteLine($"[WorldMapState] Theme available: {theme != null}");
            Console.WriteLine($"[WorldMapState] Theme.DefaultFont: {theme?.DefaultFont != null}");

            // Create HUD manager
            int screenWidth = GameServices.GraphicsDevice.Viewport.Width;
            int screenHeight = GameServices.GraphicsDevice.Viewport.Height;
            Console.WriteLine($"[WorldMapState] Screen size: {screenWidth}x{screenHeight}");

            _hud = new HUDManager(screenWidth, screenHeight);
            Console.WriteLine($"[WorldMapState] HUD created: {_hud != null}");

            // Check party
            var party = GameServices.GameData?.Party;
            Console.WriteLine($"[WorldMapState] Party available: {party != null}");
            Console.WriteLine($"[WorldMapState] Active party count: {party?.ActiveParty.Count ?? 0}");

            // Refresh party status on HUD
            _hud.RefreshPartyStatus();

            // Add welcome message
            _hud.MessageLog.AddSystemMessage("Welcome to The Mythal Prophecy!");
            _hud.MessageLog.AddSystemMessage("Press Escape or Enter to open the menu.");

            Console.WriteLine("[WorldMapState] Enter() completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WorldMapState] ERROR in Enter(): {ex.Message}");
            Console.WriteLine($"[WorldMapState] Stack trace: {ex.StackTrace}");
            throw; // Re-throw to prevent silent failure
        }
    }

    public void Exit()
    {
        // Cleanup HUD subscriptions
        _hud?.UnsubscribeFromEvents();
    }

    public void Pause()
    {
        // Hide HUD when another state is pushed on top (like pause menu)
        _hud?.Hide();
    }

    public void Resume()
    {
        // Show HUD when this state becomes active again
        _hud?.Show();

        // Reset keyboard state to prevent immediate re-triggering of menu
        _previousKeyState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState keyState = Keyboard.GetState();

        // Check for menu button (Escape or Enter)
        if ((keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape)) ||
            (keyState.IsKeyDown(Keys.Enter) && !_previousKeyState.IsKeyDown(Keys.Enter)))
        {
            // Open pause menu
            _stateManager.PushState(new GleamPauseMenuState(GameServices.Content, _stateManager));
        }

        // Press B key to manually trigger battle for testing
        if (keyState.IsKeyDown(Keys.B) && !_previousKeyState.IsKeyDown(Keys.B))
        {
            TriggerBattle();
        }

        /* DISABLED: Random encounters
        // Random encounters (10% chance per second)
        _stepCounter++;
        if (_stepCounter > 60) // Every 1 second at 60 FPS
        {
            _stepCounter = 0;
            if (_random.Next(0, 100) < 10) // 10% chance
            {
                TriggerBattle();
            }
        }
        */

        // Update HUD
        _hud?.Update(gameTime);

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // SpriteBatch is already begun by MythalGame
        try
        {
            // Draw background
            var pixel = GameServices.UI.PixelTexture;
            if (pixel != null)
            {
                var viewport = GameServices.GraphicsDevice.Viewport;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, viewport.Width, viewport.Height),
                    new Color(20, 30, 60));
            }
            else
            {
                Console.WriteLine("[WorldMapState] ERROR: PixelTexture is null");
            }

            // Draw placeholder world map text
            if (_debugFont != null)
            {
                string mapText = "WORLD MAP - Press Escape/Enter for Menu | Press B for Battle (Manual Only)";
                var textSize = _debugFont.MeasureString(mapText);
                var textPosition = new Vector2(
                    (GameServices.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                    200
                );
                spriteBatch.DrawString(_debugFont, mapText, textPosition, Color.White);

                // Draw party info with null safety
                var party = GameServices.GameData?.Party?.ActiveParty;
                if (party != null)
                {
                    string partyInfo = $"Active Party: {party.Count} members";
                    var partyTextPos = new Vector2(
                        (GameServices.GraphicsDevice.Viewport.Width - _debugFont.MeasureString(partyInfo).X) / 2,
                        250
                    );
                    spriteBatch.DrawString(_debugFont, partyInfo, partyTextPos, Color.LightGray);
                }

                // Draw gold with null safety
                int gold = GameServices.GameData?.Progress?.Gold ?? 0;
                string goldText = $"Gold: {gold}";
                var goldTextPos = new Vector2(
                    (GameServices.GraphicsDevice.Viewport.Width - _debugFont.MeasureString(goldText).X) / 2,
                    300
                );
                spriteBatch.DrawString(_debugFont, goldText, goldTextPos, Color.Gold);
            }
            else
            {
                Console.WriteLine("[WorldMapState] ERROR: _debugFont is null");
            }

            // Draw HUD (HUD components should NOT call Begin/End themselves)
            _hud?.Draw(spriteBatch, gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WorldMapState] ERROR in Draw(): {ex.Message}");
            Console.WriteLine($"[WorldMapState] Stack trace: {ex.StackTrace}");
            // Don't re-throw in Draw to prevent crash loop
        }
    }

    private void TriggerBattle()
    {
        Console.WriteLine("[WorldMapState] Triggering battle...");

        try
        {
            // Create 1-3 random enemies
            var enemyDefs = MockEnemyData.CreateEnemies();
            var enemies = new List<Entity>();
            int enemyCount = _random.Next(1, 4); // 1 to 3 enemies

            for (int i = 0; i < enemyCount; i++)
            {
                var def = enemyDefs[_random.Next(0, enemyDefs.Count)];
                var enemy = MockEnemyFactory.CreateEnemy(def);
                enemies.Add(enemy);
                Console.WriteLine($"[WorldMapState] Created enemy: {def.Name}");
            }

            // Random background theme
            var themes = new[] {
                BattleBackgroundManager.BattlegroundTheme.Ruins,
                BattleBackgroundManager.BattlegroundTheme.Castle,
                BattleBackgroundManager.BattlegroundTheme.Jungle,
                BattleBackgroundManager.BattlegroundTheme.Graveyard
            };
            var theme = themes[_random.Next(0, themes.Length)];

            Console.WriteLine($"[WorldMapState] Battle theme: {theme}");

            // Transition to battle
            _stateManager.PushState(new BattleState(_stateManager, enemies, theme));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WorldMapState] ERROR in TriggerBattle(): {ex.Message}");
            Console.WriteLine($"[WorldMapState] Stack trace: {ex.StackTrace}");
        }
    }
}
