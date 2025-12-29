using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using TheMythalProphecy.Game.Core;
using TheMythalProphecy.Game.Data.Definitions;
using TheMythalProphecy.Game.Entities.Components;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Manages target selection during battle with visual feedback
    /// </summary>
    public class BattleTargetingSystem
    {
        private readonly BattleContext _context;
        private readonly BattleManager _battleManager;
        private TargetingMode _currentMode;
        private int _selectedIndex;
        private List<Combatant> _validTargets;
        private BattleActionType _currentActionType;
        private Combatant _currentActor;
        private string _itemId;
        private bool _justConfirmed;

        public bool IsActive { get; private set; }
        public Combatant CurrentTarget { get; private set; }
        public List<Combatant> CurrentTargets { get; private set; }
        public TargetingMode CurrentMode => _currentMode;
        public bool JustConfirmed => _justConfirmed;

        public BattleTargetingSystem(BattleContext context, BattleManager battleManager)
        {
            _context = context;
            _battleManager = battleManager;
            _validTargets = new List<Combatant>();
            CurrentTargets = new List<Combatant>();
            IsActive = false;
            _justConfirmed = false;
        }

        /// <summary>
        /// Clear the just confirmed flag (call this each frame)
        /// </summary>
        public void ClearJustConfirmed()
        {
            _justConfirmed = false;
        }

        /// <summary>
        /// Activate targeting mode for a specific action
        /// </summary>
        public void ActivateTargeting(BattleActionType actionType, Combatant actor, string itemId = null)
        {
            Console.WriteLine($"[Targeting] Activating targeting for {actionType}, Actor: {actor.Name}");

            _currentActionType = actionType;
            _currentActor = actor;
            _itemId = itemId;
            IsActive = true;

            // Determine targeting mode
            _currentMode = DetermineTargetingMode(actionType, itemId);
            Console.WriteLine($"[Targeting] Mode: {_currentMode}");

            // Update valid targets based on mode
            UpdateValidTargets();
            Console.WriteLine($"[Targeting] Found {_validTargets.Count} valid targets");

            // If no valid targets, deactivate immediately
            if (_validTargets.Count == 0)
            {
                Console.WriteLine("[Targeting] No valid targets found - deactivating");
                DeactivateTargeting();
                return;
            }

            // Select first target by default
            _selectedIndex = 0;
            CurrentTarget = _validTargets[0];
            Console.WriteLine($"[Targeting] Selected default target: {CurrentTarget.Name}");

            // Check if this is multi-target mode
            if (IsMultiTargetMode())
            {
                CurrentTargets = new List<Combatant>(_validTargets);
                Console.WriteLine($"[Targeting] Multi-target mode - {CurrentTargets.Count} targets selected");
            }
            else
            {
                CurrentTargets = new List<Combatant> { CurrentTarget };
                Console.WriteLine($"[Targeting] Single-target mode - 1 target selected");
            }

            // Apply flash effects
            ApplyFlashEffects();
        }

        /// <summary>
        /// Deactivate targeting mode and clean up
        /// </summary>
        public void DeactivateTargeting()
        {
            RemoveFlashEffects();
            CurrentTarget = null;
            CurrentTargets.Clear();
            _validTargets.Clear();
            IsActive = false;
        }

        /// <summary>
        /// Handle input during targeting mode
        /// </summary>
        public void HandleInput(KeyboardState currentState, KeyboardState previousState)
        {
            if (!IsActive)
                return;

            // Only allow navigation in single-target mode
            if (!IsMultiTargetMode())
            {
                // Up arrow - select previous target
                if (currentState.IsKeyDown(Keys.Up) && !previousState.IsKeyDown(Keys.Up))
                {
                    SelectPreviousTarget();
                }

                // Down arrow - select next target
                if (currentState.IsKeyDown(Keys.Down) && !previousState.IsKeyDown(Keys.Down))
                {
                    SelectNextTarget();
                }
            }

            // A key - confirm selection
            if (currentState.IsKeyDown(Keys.A) && !previousState.IsKeyDown(Keys.A))
            {
                Console.WriteLine("[Targeting] A key pressed - confirming selection");
                ConfirmSelection();
            }

            // Escape - cancel targeting
            if (currentState.IsKeyDown(Keys.Escape) && !previousState.IsKeyDown(Keys.Escape))
            {
                DeactivateTargeting();
            }
        }

        /// <summary>
        /// Select the next target in the list (with wrap-around)
        /// </summary>
        public void SelectNextTarget()
        {
            if (_validTargets.Count == 0 || IsMultiTargetMode())
                return;

            _selectedIndex = (_selectedIndex + 1) % _validTargets.Count;
            CurrentTarget = _validTargets[_selectedIndex];
            CurrentTargets = new List<Combatant> { CurrentTarget };
            ApplyFlashEffects();
        }

        /// <summary>
        /// Select the previous target in the list (with wrap-around)
        /// </summary>
        public void SelectPreviousTarget()
        {
            if (_validTargets.Count == 0 || IsMultiTargetMode())
                return;

            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _validTargets.Count - 1;

            CurrentTarget = _validTargets[_selectedIndex];
            CurrentTargets = new List<Combatant> { CurrentTarget };
            ApplyFlashEffects();
        }

        /// <summary>
        /// Confirm the current selection and execute the action
        /// </summary>
        public void ConfirmSelection()
        {
            if (CurrentTargets.Count == 0)
            {
                Console.WriteLine("[Targeting] Cannot confirm - no targets selected!");
                return;
            }

            Console.WriteLine($"[Targeting] Confirming selection: {CurrentTargets.Count} target(s)");
            foreach (var target in CurrentTargets)
            {
                Console.WriteLine($"  - Target: {target.Name} (HP: {target.Stats.GetStat(Characters.Stats.StatType.HP)})");
            }

            // Create battle action
            var action = new BattleAction(_currentActionType, _currentActor, CurrentTargets);

            // Set item ID if this is an item action
            if (_currentActionType == BattleActionType.UseItem && !string.IsNullOrEmpty(_itemId))
            {
                action.ItemId = _itemId;
            }

            Console.WriteLine($"[Targeting] Created action: {action.ActionType}, Actor: {action.Actor.Name}, Targets: {action.Targets.Count}");

            // Process the action
            _battleManager.ProcessPlayerAction(action);

            // Set flag to prevent immediate reactivation
            _justConfirmed = true;

            // Deactivate targeting
            DeactivateTargeting();
        }

        /// <summary>
        /// Update the list of valid targets based on current mode
        /// </summary>
        private void UpdateValidTargets()
        {
            _validTargets.Clear();

            switch (_currentMode)
            {
                case TargetingMode.SingleEnemy:
                case TargetingMode.AllEnemies:
                    _validTargets = _context.GetAliveEnemies(_currentActor);
                    break;

                case TargetingMode.SingleAlly:
                case TargetingMode.AllAllies:
                    _validTargets = _context.GetAliveAllies(_currentActor);
                    break;

                case TargetingMode.Self:
                    _validTargets = new List<Combatant> { _currentActor };
                    break;

                case TargetingMode.Any:
                    _validTargets = _context.GetAliveCombatants();
                    break;
            }
        }

        /// <summary>
        /// Apply flash effects to currently selected target(s)
        /// </summary>
        private void ApplyFlashEffects()
        {
            // First remove all existing flash effects
            RemoveFlashEffects();

            // Add flash component to each selected target
            foreach (var target in CurrentTargets)
            {
                var flashComponent = new TargetFlashComponent();
                flashComponent.Initialize();
                target.Entity.AddComponent(flashComponent);
            }
        }

        /// <summary>
        /// Remove all flash effects from all combatants
        /// </summary>
        private void RemoveFlashEffects()
        {
            foreach (var combatant in _context.AllCombatants)
            {
                var flashComponent = combatant.Entity.GetComponent<TargetFlashComponent>();
                if (flashComponent != null)
                {
                    combatant.Entity.RemoveComponent<TargetFlashComponent>();
                }
            }
        }

        /// <summary>
        /// Determine the targeting mode based on action type and item properties
        /// </summary>
        private TargetingMode DetermineTargetingMode(BattleActionType actionType, string itemId)
        {
            switch (actionType)
            {
                case BattleActionType.Attack:
                    return TargetingMode.SingleEnemy;

                case BattleActionType.UseItem:
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var itemDef = GameServices.GameData.ItemDatabase.Get(itemId);
                        if (itemDef != null)
                        {
                            // Check if multi-target
                            if (itemDef.IsMultiTarget)
                            {
                                return itemDef.ItemTargetType == TargetType.Ally
                                    ? TargetingMode.AllAllies
                                    : TargetingMode.AllEnemies;
                            }

                            // Single target
                            return itemDef.ItemTargetType switch
                            {
                                TargetType.Ally => TargetingMode.SingleAlly,
                                TargetType.Enemy => TargetingMode.SingleEnemy,
                                TargetType.Self => TargetingMode.Self,
                                TargetType.Any => TargetingMode.Any,
                                _ => TargetingMode.SingleAlly
                            };
                        }
                    }
                    // Default for items is single ally (healing)
                    return TargetingMode.SingleAlly;

                case BattleActionType.Skill:
                    // TODO: When skill system is implemented, check skill definition
                    return TargetingMode.SingleEnemy;

                case BattleActionType.Defend:
                case BattleActionType.Flee:
                default:
                    return TargetingMode.Self;
            }
        }

        /// <summary>
        /// Check if current mode is multi-target
        /// </summary>
        private bool IsMultiTargetMode()
        {
            return _currentMode == TargetingMode.AllEnemies || _currentMode == TargetingMode.AllAllies;
        }
    }

    /// <summary>
    /// Defines the scope of target selection
    /// </summary>
    public enum TargetingMode
    {
        SingleEnemy,    // Attack, damage items
        SingleAlly,     // Healing items, buffs
        AllEnemies,     // AoE damage skills
        AllAllies,      // Party-wide buffs
        Self,           // Self-targeting only
        Any             // Flexible targeting
    }
}
