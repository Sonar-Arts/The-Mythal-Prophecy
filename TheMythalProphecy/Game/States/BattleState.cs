using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Battle;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Systems.Events;
using TheMythalProphecy.Game.Systems.Rendering;

namespace TheMythalProphecy.Game.States
{
    /// <summary>
    /// Main battle game state - turn-based combat with keyboard controls
    /// </summary>
    public class BattleState : IGameState
    {
        private readonly GameStateManager _stateManager;
        private BattleManager _battleManager;
        private BattleContext _battleContext;
        private BattleBackground _background;
        private KeyboardState _previousKeyState;
        private SpriteFont _font;
        private string _message;

        // Battle data
        private readonly List<Entity> _enemies;
        private readonly BattleBackgroundManager.BattlegroundTheme _theme;
        private bool _battleComplete = false;
        private bool _debugPrinted = false;

        public BattleState(GameStateManager stateManager, List<Entity> enemies, BattleBackgroundManager.BattlegroundTheme theme)
        {
            _stateManager = stateManager;
            _enemies = enemies;
            _theme = theme;
            _message = "Battle Start!";
        }

        public void Enter()
        {
            // Load font
            _font = GameServices.Content.Load<SpriteFont>("Fonts/Default");

            // Load battle background
            _background = BattleBackgroundManager.LoadBattleground(_theme);

            // Get party from GameData
            var party = GameServices.GameData.Party.ActiveParty.ToList();

            // Create battle context
            _battleContext = new BattleContext(party, _enemies, _theme);

            // Create battle manager
            _battleManager = new BattleManager(_battleContext);

            // Subscribe to events
            GameServices.Events.Subscribe<CharacterDefeatedEvent>(OnCharacterDefeated);
            GameServices.Events.Subscribe<LevelUpEvent>(OnLevelUp);
            GameServices.Events.Subscribe<DamageDealtEvent>(OnDamageDealt);

            // Publish combat started event
            GameServices.Events.Publish(new CombatStartedEvent(_enemies.Cast<object>().ToList()));

            _previousKeyState = Keyboard.GetState();

            Console.WriteLine($"Battle started! {_battleContext.PlayerCombatants.Count} heroes vs {_battleContext.EnemyCombatants.Count} enemies");

            // Debug: List all combatants
            Console.WriteLine("=== Player Combatants ===");
            foreach (var combatant in _battleContext.PlayerCombatants)
            {
                Console.WriteLine($"  {combatant.Name} at position {combatant.BattlePosition} (Entity ID: {combatant.Entity.Id})");
            }

            Console.WriteLine("=== Enemy Combatants ===");
            foreach (var combatant in _battleContext.EnemyCombatants)
            {
                Console.WriteLine($"  {combatant.Name} at position {combatant.BattlePosition} (Entity ID: {combatant.Entity.Id})");
            }
        }

        public void Exit()
        {
            // Unsubscribe from events
            GameServices.Events.Unsubscribe<CharacterDefeatedEvent>(OnCharacterDefeated);
            GameServices.Events.Unsubscribe<LevelUpEvent>(OnLevelUp);
            GameServices.Events.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
        }

        public void Pause()
        {
            // Do nothing
        }

        public void Resume()
        {
            _previousKeyState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime)
        {
            // Update background parallax (pass null camera for now)
            _background?.Update(gameTime, null);

            // Update battle manager
            _battleManager.Update(gameTime);

            // Update combatant positions and animations
            UpdateCombatantComponents(gameTime);

            // Handle input based on current phase
            var currentPhase = _battleManager.CurrentPhase;

            if (currentPhase == BattlePhase.PlayerInput)
            {
                HandlePlayerInput();
            }

            // Handle keyboard shortcuts
            KeyboardState keyState = Keyboard.GetState();

            // ESC for pause menu
            if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
            {
                _stateManager.PushState(new GleamPauseMenuState(GameServices.Content, _stateManager));
            }

            _previousKeyState = keyState;

            // Check if battle is complete
            if (_battleManager.IsBattleComplete() && !_battleComplete)
            {
                _battleComplete = true;

                if (_battleContext.Result.Victory)
                {
                    HandleVictoryComplete();
                }
                else
                {
                    HandleDefeatComplete();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var viewport = GameServices.GraphicsDevice.Viewport;

            // Draw battle background
            _background?.Draw(spriteBatch, viewport);

            // Draw combatants (simple colored rectangles)
            DrawCombatants(spriteBatch);

            // Draw battle UI text
            DrawBattleText(spriteBatch);
        }

        private void HandlePlayerInput()
        {
            KeyboardState keyState = Keyboard.GetState();
            var currentCombatant = _battleManager.CurrentTurnCombatant;

            if (currentCombatant == null || !currentCombatant.IsPlayer)
                return;

            // A = Attack
            if (keyState.IsKeyDown(Keys.A) && !_previousKeyState.IsKeyDown(Keys.A))
            {
                var enemies = _battleContext.GetAliveEnemies(currentCombatant);
                if (enemies.Count > 0)
                {
                    var target = enemies[0]; // Attack first enemy
                    var action = new BattleAction(BattleActionType.Attack, currentCombatant, target);
                    _battleManager.ProcessPlayerAction(action);
                }
            }

            // D = Defend
            if (keyState.IsKeyDown(Keys.D) && !_previousKeyState.IsKeyDown(Keys.D))
            {
                var action = new BattleAction(BattleActionType.Defend, currentCombatant, new List<Combatant>());
                _battleManager.ProcessPlayerAction(action);
            }

            // I = Use Item (potion on self)
            if (keyState.IsKeyDown(Keys.I) && !_previousKeyState.IsKeyDown(Keys.I))
            {
                var inventory = GameServices.GameData.Inventory;
                if (inventory.GetItemCount("potion") > 0)
                {
                    var action = new BattleAction(BattleActionType.UseItem, currentCombatant, currentCombatant);
                    action.ItemId = "potion";
                    _battleManager.ProcessPlayerAction(action);
                }
                else
                {
                    _message = "No potions!";
                }
            }

            // F = Flee
            if (keyState.IsKeyDown(Keys.F) && !_previousKeyState.IsKeyDown(Keys.F))
            {
                var action = new BattleAction(BattleActionType.Flee, currentCombatant, new List<Combatant>());
                _battleManager.ProcessPlayerAction(action);
            }
        }

        private void DrawCombatants(SpriteBatch spriteBatch)
        {
            // Draw party members (left side) - only alive combatants
            foreach (var combatant in _battleContext.PlayerCombatants)
            {
                // Only draw if alive
                if (!combatant.IsAlive)
                    continue;

                DrawCombatantSprite(spriteBatch, combatant, flipHorizontal: false);
            }

            // Draw enemies (right side) - flip them to face left, only alive
            foreach (var combatant in _battleContext.EnemyCombatants)
            {
                // Only draw if alive
                if (!combatant.IsAlive)
                    continue;

                DrawCombatantSprite(spriteBatch, combatant, flipHorizontal: true);
            }
        }

        /// <summary>
        /// Draw a single combatant sprite with animation
        /// </summary>
        private void DrawCombatantSprite(SpriteBatch spriteBatch, Combatant combatant, bool flipHorizontal)
        {
            var sprite = combatant.Entity.GetComponent<SpriteComponent>();
            var transform = combatant.Entity.GetComponent<TransformComponent>();
            var animation = combatant.Animation;

            if (sprite == null || transform == null || sprite.Texture == null)
            {
                // Fallback to rectangle if sprite not available
                var rect = new Rectangle((int)combatant.BattlePosition.X, (int)combatant.BattlePosition.Y, 50, 50);
                spriteBatch.Draw(GameServices.UI.PixelTexture, rect,
                    combatant.IsPlayer ? Color.Blue : Color.Red);
                return;
            }

            // Debug: Log sprite info for first combatant only (once)
            if (!_debugPrinted && combatant == _battleContext.PlayerCombatants[0])
            {
                _debugPrinted = true;
                Console.WriteLine($"[Sprite Debug] {combatant.Name}:");
                Console.WriteLine($"  Texture: {sprite.Texture.Width}x{sprite.Texture.Height}");
                Console.WriteLine($"  SourceRect: {sprite.SourceRectangle}");
                Console.WriteLine($"  Origin: {sprite.Origin}");
                Console.WriteLine($"  Position: {transform.Position}");
                Console.WriteLine($"  Scale: 2.0");

                // Also check the animation state
                if (animation != null)
                {
                    Console.WriteLine($"  Animation State: {animation.CurrentState}");
                    Console.WriteLine($"  Animation Playing: {animation.CurrentAnimation?.IsPlaying ?? false}");
                }
            }

            // Draw the sprite
            spriteBatch.Draw(
                sprite.Texture,
                transform.Position,
                sprite.SourceRectangle,
                combatant.IsDead ? Color.Gray : Color.White, // Gray tint when dead
                transform.Rotation,
                sprite.Origin,
                2.0f, // Scale up to make sprites visible (128px frames)
                flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                sprite.LayerDepth
            );
        }

        private void DrawBattleText(SpriteBatch spriteBatch)
        {
            var viewport = GameServices.GraphicsDevice.Viewport;
            int y = 20;

            // Draw turn info
            var currentCombatant = _battleManager.CurrentTurnCombatant;
            if (currentCombatant != null)
            {
                string turnText = $"Turn: {currentCombatant.Name}'s turn";
                spriteBatch.DrawString(_font, turnText, new Vector2(viewport.Width / 2 - 100, y), Color.White);
                y += 30;
            }

            // Draw controls during player turn
            if (_battleManager.CurrentPhase == BattlePhase.PlayerInput)
            {
                string controls = "A=Attack D=Defend I=Item F=Flee";
                spriteBatch.DrawString(_font, controls, new Vector2(viewport.Width / 2 - 150, y), Color.Yellow);
                y += 30;
            }

            // Draw message
            spriteBatch.DrawString(_font, _message, new Vector2(viewport.Width / 2 - 100, viewport.Height - 60), Color.White);

            // Draw HP for party (left side)
            int partyY = 100;
            foreach (var combatant in _battleContext.PlayerCombatants)
            {
                var stats = combatant.Stats;
                string hpText = $"{combatant.Name}: {stats.GetStat(Characters.Stats.StatType.HP)}/{stats.GetStat(Characters.Stats.StatType.MaxHP)} HP";
                spriteBatch.DrawString(_font, hpText, new Vector2(50, partyY), combatant.IsAlive ? Color.White : Color.Gray);
                partyY += 100;
            }

            // Draw HP for enemies (right side)
            int enemyY = 150;
            foreach (var combatant in _battleContext.EnemyCombatants)
            {
                var stats = combatant.Stats;
                string hpText = combatant.IsAlive
                    ? $"{combatant.Name}: {stats.GetStat(Characters.Stats.StatType.HP)}/{stats.GetStat(Characters.Stats.StatType.MaxHP)} HP"
                    : $"{combatant.Name}: DEFEATED";
                spriteBatch.DrawString(_font, hpText, new Vector2(viewport.Width - 250, enemyY), combatant.IsAlive ? Color.White : Color.Gray);
                enemyY += 100;
            }
        }

        private void HandleVictoryComplete()
        {
            // Distribute XP to party
            foreach (var kvp in _battleContext.Result.ExperiencePerCharacter)
            {
                var entity = kvp.Key;
                var xp = kvp.Value;
                var stats = entity.GetComponent<StatsComponent>();
                stats?.AddExperience(xp);
            }

            // Add gold
            GameServices.GameData.Progress.Gold += _battleContext.Result.GoldEarned;

            // Add items to inventory
            foreach (var itemId in _battleContext.Result.ItemsEarned)
            {
                GameServices.GameData.Inventory.AddItem(itemId, 1);
            }

            // Publish combat ended event
            GameServices.Events.Publish(new CombatEndedEvent(true, _battleContext.Result));

            Console.WriteLine($"Victory! Earned {_battleContext.Result.GoldEarned} gold and {_battleContext.Result.ExperienceEarned} XP");

            // Return to world map
            _stateManager.PopState();
        }

        private void HandleDefeatComplete()
        {
            // Publish combat ended event
            GameServices.Events.Publish(new CombatEndedEvent(false, _battleContext.Result));

            Console.WriteLine("Defeat! Game Over");

            // Return to title screen
            _stateManager.ChangeState(new TitleScreenState(GameServices.Content, _stateManager));
        }

        /// <summary>
        /// Update combatant entity components for rendering
        /// </summary>
        private void UpdateCombatantComponents(GameTime gameTime)
        {
            // Only update alive combatants
            foreach (var combatant in _battleContext.AllCombatants)
            {
                // Skip dead combatants - don't update their animations or positions
                if (!combatant.IsAlive)
                    continue;

                // Sync TransformComponent position with BattlePosition
                var transform = combatant.Entity.GetComponent<TransformComponent>();
                if (transform != null)
                {
                    transform.Position = combatant.BattlePosition;
                }

                // Update animation component
                var animation = combatant.Animation;
                if (animation != null)
                {
                    animation.Update(gameTime);
                }

                // Sync sprite component with current animation frame
                var sprite = combatant.Entity.GetComponent<SpriteComponent>();
                if (sprite != null && animation != null)
                {
                    sprite.SourceRectangle = animation.GetCurrentFrameRectangle();
                    sprite.Texture = animation.GetCurrentTexture();

                    // Update origin to center of current frame
                    if (!sprite.SourceRectangle.IsEmpty)
                    {
                        sprite.Origin = new Vector2(
                            sprite.SourceRectangle.Width / 2f,
                            sprite.SourceRectangle.Height / 2f
                        );
                    }
                }
            }
        }

        // Event Handlers
        private void OnCharacterDefeated(CharacterDefeatedEvent e)
        {
            var entity = e.Character as Entity;
            _message = $"{entity?.Name ?? "Someone"} defeated!";
        }

        private void OnLevelUp(LevelUpEvent e)
        {
            var entity = e.Character as Entity;
            _message = $"{entity?.Name ?? "Someone"} Level {e.NewLevel}!";
        }

        private void OnDamageDealt(DamageDealtEvent e)
        {
            var entity = e.Target as Entity;
            string critText = e.IsCritical ? " CRIT!" : "";
            _message = $"{entity?.Name ?? "Target"} {e.Amount} dmg{critText}";
        }
    }
}
