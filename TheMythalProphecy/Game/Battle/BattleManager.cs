using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TheMythalProphecy.Game.Characters.Stats;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Data.Mock;
using TheMythalProphecy.Game.Entities.Components;
using TheMythalProphecy.Game.Systems.Events;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Orchestrates battle flow and phase transitions
    /// </summary>
    public class BattleManager
    {
        private readonly BattleContext _context;
        private readonly TurnOrderSystem _turnOrder;
        private BattlePhase _currentPhase;
        private float _actionAnimationTimer;
        private const float ACTION_ANIMATION_DURATION = 1.5f;

        public BattlePhase CurrentPhase => _currentPhase;
        public Combatant CurrentTurnCombatant => _context.CurrentTurnCombatant;

        public BattleManager(BattleContext context)
        {
            _context = context;
            _turnOrder = new TurnOrderSystem(context.AllCombatants);
            _currentPhase = BattlePhase.Initialize;
            _actionAnimationTimer = 0f;
        }

        /// <summary>
        /// Main update loop - switches on current phase
        /// </summary>
        public void Update(GameTime gameTime)
        {
            switch (_currentPhase)
            {
                case BattlePhase.Initialize:
                    UpdateInitializePhase();
                    break;

                case BattlePhase.TurnStart:
                    UpdateTurnStartPhase();
                    break;

                case BattlePhase.PlayerInput:
                    UpdatePlayerInputPhase();
                    break;

                case BattlePhase.EnemyAI:
                    UpdateEnemyAIPhase();
                    break;

                case BattlePhase.ExecutingAction:
                    UpdateExecutingActionPhase(gameTime);
                    break;

                case BattlePhase.CheckVictory:
                    UpdateCheckVictoryPhase();
                    break;

                case BattlePhase.Victory:
                    UpdateVictoryPhase();
                    break;

                case BattlePhase.Defeat:
                    UpdateDefeatPhase();
                    break;
            }
        }

        /// <summary>
        /// Initialize phase - one-time setup
        /// </summary>
        private void UpdateInitializePhase()
        {
            _turnOrder.CalculateInitialTurnOrder();
            _currentPhase = BattlePhase.TurnStart;
        }

        /// <summary>
        /// Turn start phase - select next combatant
        /// </summary>
        private void UpdateTurnStartPhase()
        {
            // Get next combatant from turn order
            _context.CurrentTurnCombatant = _turnOrder.GetNextTurn();

            if (_context.CurrentTurnCombatant == null)
            {
                // No combatants left - shouldn't happen
                _currentPhase = BattlePhase.CheckVictory;
                return;
            }

            // Update status effects (DoT/HoT) - handled automatically in StatsComponent

            // Check if combatant is still alive after status effects
            if (_context.CurrentTurnCombatant.IsDead)
            {
                // Skip dead combatant's turn
                _currentPhase = BattlePhase.CheckVictory;
                return;
            }

            // Publish turn started event
            GameServices.Events.Publish(new TurnStartedEvent(_context.CurrentTurnCombatant, _context.TurnNumber));

            // Transition based on whether it's player or enemy turn
            if (_context.CurrentTurnCombatant.IsPlayer)
            {
                _currentPhase = BattlePhase.PlayerInput;
            }
            else
            {
                _currentPhase = BattlePhase.EnemyAI;
            }
        }

        /// <summary>
        /// Player input phase - wait for player to select action
        /// Handled by BattleState
        /// </summary>
        private void UpdatePlayerInputPhase()
        {
            // Waiting for player input
            // BattleState will call ProcessPlayerAction() when ready
        }

        /// <summary>
        /// Enemy AI phase - minimal random attack AI
        /// </summary>
        private void UpdateEnemyAIPhase()
        {
            var enemy = _context.CurrentTurnCombatant;

            // Simple AI: Select random alive player target and attack
            var playerTargets = _context.GetAliveEnemies(enemy);

            if (playerTargets.Count == 0)
            {
                // No targets available
                _currentPhase = BattlePhase.CheckVictory;
                return;
            }

            var target = TargetSelector.SelectRandomTarget(playerTargets);
            var action = new BattleAction(BattleActionType.Attack, enemy, target);

            enemy.SetAction(action);

            // Execute immediately
            _currentPhase = BattlePhase.ExecutingAction;
        }

        /// <summary>
        /// Executing action phase - play animations and apply effects
        /// </summary>
        private void UpdateExecutingActionPhase(GameTime gameTime)
        {
            var action = _context.CurrentTurnCombatant.QueuedAction;

            if (action == null)
            {
                Console.WriteLine($"[BattleManager] ExecutingAction phase but action is null - transitioning to CheckVictory");
                _currentPhase = BattlePhase.CheckVictory;
                return;
            }

            // Execute action on first frame
            if (!action.IsExecuted)
            {
                // Execute the action
                ExecuteAction(action);

                // Mark as executed
                action.IsExecuted = true;

                // Start animation timer
                _actionAnimationTimer = ACTION_ANIMATION_DURATION;
            }

            // Wait for animation to complete (runs every frame)
            _actionAnimationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_actionAnimationTimer <= 0)
            {
                // Animation complete, check victory
                _actionAnimationTimer = 0;
                _currentPhase = BattlePhase.CheckVictory;
            }
        }

        /// <summary>
        /// Check victory phase - evaluate win/loss conditions
        /// </summary>
        private void UpdateCheckVictoryPhase()
        {
            if (_context.AreAllEnemiesDead())
            {
                _currentPhase = BattlePhase.Victory;
            }
            else if (_context.AreAllPlayersDead())
            {
                _currentPhase = BattlePhase.Defeat;
            }
            else
            {
                // Continue battle - next turn
                _currentPhase = BattlePhase.TurnStart;
            }
        }

        /// <summary>
        /// Victory phase - calculate and store rewards
        /// </summary>
        private void UpdateVictoryPhase()
        {
            if (_context.Result.Victory)
                return; // Already calculated

            // Calculate rewards
            int totalGold = 0;
            int totalExperience = 0;

            foreach (var enemy in _context.EnemyCombatants)
            {
                var rewards = MockEnemyFactory.GetRewards(enemy.Entity.Id);

                if (rewards != null)
                {
                    // Roll gold
                    totalGold += DamageCalculator.RollGoldReward(rewards);

                    // Add experience
                    totalExperience += rewards.Experience;

                    // Roll for item drops
                    var droppedItem = DamageCalculator.RollItemDrop(rewards);
                    if (!string.IsNullOrEmpty(droppedItem))
                    {
                        _context.Result.ItemsEarned.Add(droppedItem);
                    }
                }
            }

            _context.Result.Victory = true;
            _context.Result.GoldEarned = totalGold;
            _context.Result.ExperienceEarned = totalExperience;

            // Divide experience among surviving party members
            int xpPerMember = totalExperience / Math.Max(1, _context.PlayerCombatants.Count(c => c.IsAlive));

            foreach (var player in _context.PlayerCombatants.Where(c => c.IsAlive))
            {
                _context.Result.ExperiencePerCharacter[player.Entity] = xpPerMember;
            }
        }

        /// <summary>
        /// Defeat phase - mark battle as lost
        /// </summary>
        private void UpdateDefeatPhase()
        {
            _context.Result.Victory = false;
        }

        /// <summary>
        /// Process player action from BattleState
        /// </summary>
        public void ProcessPlayerAction(BattleAction action)
        {
            if (_currentPhase != BattlePhase.PlayerInput)
            {
                return;
            }

            _context.CurrentTurnCombatant.SetAction(action);
            _currentPhase = BattlePhase.ExecutingAction;
        }

        /// <summary>
        /// Execute a battle action
        /// </summary>
        private void ExecuteAction(BattleAction action)
        {
            switch (action.ActionType)
            {
                case BattleActionType.Attack:
                    ExecuteAttack(action);
                    break;

                case BattleActionType.Defend:
                    ExecuteDefend(action);
                    break;

                case BattleActionType.UseItem:
                    ExecuteItemUse(action);
                    break;

                case BattleActionType.Flee:
                    ExecuteFlee(action);
                    break;
            }

            // Publish action executed event
            GameServices.Events.Publish(new ActionExecutedEvent(action));
        }

        /// <summary>
        /// Execute attack action
        /// </summary>
        private void ExecuteAttack(BattleAction action)
        {
            var attacker = action.Actor;

            // Set attacker animation
            if (attacker.Animation != null)
            {
                attacker.Animation.CurrentState = AnimationState.Attack;
            }

            foreach (var target in action.Targets)
            {
                if (target.IsDead)
                    continue;

                // Calculate damage
                var damageResult = DamageCalculator.CalculatePhysicalDamage(attacker, target);

                if (damageResult.IsEvaded)
                {
                    // Evaded - play dodge animation
                    if (target.Animation != null)
                    {
                        target.Animation.PlayOneShotAnimation(AnimationState.Dodge);
                    }
                    Console.WriteLine($"{target.Name} evaded the attack!");
                }
                else
                {
                    // Apply damage
                    target.Stats.TakeDamage(damageResult.Damage);

                    // Play hurt or death animation
                    if (target.Animation != null)
                    {
                        if (target.IsDead)
                        {
                            target.Animation.CurrentState = AnimationState.Death;
                            target.State = CombatantState.Dead;
                        }
                        else
                        {
                            target.Animation.PlayOneShotAnimation(AnimationState.Hurt);
                        }
                    }

                    // Publish damage event
                    GameServices.Events.Publish(new DamageDealtEvent(target.Entity, damageResult.Damage, damageResult.IsCritical));

                    string critText = damageResult.IsCritical ? " Critical!" : "";
                    string blockText = damageResult.IsBlocked ? " (Blocked)" : "";
                    Console.WriteLine($"{attacker.Name} attacks {target.Name} for {damageResult.Damage} damage{critText}{blockText}");

                    // Check if target died
                    if (target.IsDead)
                    {
                        GameServices.Events.Publish(new CharacterDefeatedEvent(target.Entity));
                        Console.WriteLine($"{target.Name} has been defeated!");
                    }
                }
            }
        }

        /// <summary>
        /// Execute defend action
        /// </summary>
        private void ExecuteDefend(BattleAction action)
        {
            var defender = action.Actor;

            // Set defending state
            defender.State = CombatantState.Defending;

            // Play defend animation
            if (defender.Animation != null)
            {
                defender.Animation.CurrentState = AnimationState.Defend;
            }

            // Apply defense buff (temporary status effect)
            var defenseBuff = new StatusEffect(
                StatusEffectType.Protect,
                "Defending",
                3f, // 3 seconds duration
                isBuff: true
            );
            defenseBuff.AddStatModifier(StatType.Defense, 10); // +10 defense

            defender.Stats?.AddStatusEffect(defenseBuff);

            Console.WriteLine($"{defender.Name} takes a defensive stance!");
        }

        /// <summary>
        /// Execute item use action
        /// </summary>
        private void ExecuteItemUse(BattleAction action)
        {
            var user = action.Actor;
            var itemId = action.ItemId;

            // Get item from inventory
            var inventory = GameServices.GameData.Inventory;
            var itemDef = GameServices.GameData.ItemDatabase.Get(itemId);

            if (itemDef == null)
            {
                Console.WriteLine($"Item {itemId} not found!");
                return;
            }

            // Check if item is in inventory
            if (inventory.GetItemCount(itemId) <= 0)
            {
                Console.WriteLine($"No {itemDef.Name} in inventory!");
                return;
            }

            // Play item use animation
            if (user.Animation != null)
            {
                user.Animation.PlayOneShotAnimation(AnimationState.Heal);
            }

            // Apply item effects (healing potions only for Phase 5)
            if (itemDef.HPRestore > 0)
            {
                foreach (var target in action.Targets)
                {
                    int healing = DamageCalculator.CalculateHealing(
                        itemDef.HPRestore,
                        user.Stats?.GetStat(StatType.MagicPower) ?? 0
                    );

                    target.Stats?.Heal(healing);

                    GameServices.Events.Publish(new HealingAppliedEvent(target.Entity, healing));
                    Console.WriteLine($"{user.Name} uses {itemDef.Name} on {target.Name} for {healing} HP!");
                }
            }

            // Remove item from inventory
            inventory.RemoveItem(itemId, 1);
            GameServices.Events.Publish(new ItemUsedEvent(itemId, user.Entity, 1));
        }

        /// <summary>
        /// Execute flee action
        /// </summary>
        private void ExecuteFlee(BattleAction action)
        {
            var fleer = action.Actor;

            // Calculate flee success
            bool success = DamageCalculator.CalculateFleeSuccess(_context);

            if (success)
            {
                Console.WriteLine($"{fleer.Name} successfully fled from battle!");

                // Mark as defeat with no rewards
                _context.Result.Victory = false;
                _currentPhase = BattlePhase.Defeat;
            }
            else
            {
                Console.WriteLine($"{fleer.Name} failed to flee!");
                // Turn wasted, continue battle
            }
        }

        /// <summary>
        /// Checks if battle is complete
        /// </summary>
        public bool IsBattleComplete()
        {
            return _currentPhase == BattlePhase.Victory || _currentPhase == BattlePhase.Defeat;
        }
    }

    /// <summary>
    /// Battle phase enumeration
    /// </summary>
    public enum BattlePhase
    {
        Initialize,
        TurnStart,
        PlayerInput,
        EnemyAI,
        ExecutingAction,
        CheckVictory,
        Victory,
        Defeat
    }
}
