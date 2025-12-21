using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheMythalProphecy.Game.UI.Components;
using TheMythalProphecy.Game.Systems.Events;
using TheMythalProphecy.Game.Entities.Components;

namespace TheMythalProphecy.Game.UI.HUD
{
    /// <summary>
    /// Manages all HUD elements and shows/hides them based on game state.
    /// Subscribes to game events for automatic updates.
    /// </summary>
    public class HUDManager
    {
        private List<UIElement> _hudElements;
        private PartyStatusHUD _partyStatusHud;
        private MessageLog _messageLog;
        private bool _isVisible;
        private bool _eventsSubscribed;

        public PartyStatusHUD PartyStatus => _partyStatusHud;
        public MessageLog MessageLog => _messageLog;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                foreach (var element in _hudElements)
                {
                    element.Visible = value;
                }
            }
        }

        public HUDManager(int screenWidth, int screenHeight)
        {
            _hudElements = new List<UIElement>();
            _isVisible = true;
            _eventsSubscribed = false;

            // Create HUD elements
            _partyStatusHud = new PartyStatusHUD(
                new Vector2(10, screenHeight - 120),
                new Vector2(screenWidth - 20, 110)
            );
            _messageLog = new MessageLog(
                new Vector2(10, 10),
                new Vector2(400, 150)
            );

            _hudElements.Add(_partyStatusHud);
            _hudElements.Add(_messageLog);

            // Subscribe to game events
            SubscribeToEvents();
        }

        /// <summary>
        /// Subscribe to relevant game events for automatic HUD updates
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventsSubscribed) return;

            var eventManager = Core.GameServices.Events;
            if (eventManager == null) return;

            // Party events
            eventManager.Subscribe<PartyChangedEvent>(OnPartyChanged);

            // Combat events
            eventManager.Subscribe<DamageDealtEvent>(OnDamageDealt);
            eventManager.Subscribe<HealingAppliedEvent>(OnHealingApplied);
            eventManager.Subscribe<CombatStartedEvent>(OnCombatStarted);

            // Character events
            eventManager.Subscribe<LevelUpEvent>(OnLevelUp);
            eventManager.Subscribe<SkillLearnedEvent>(OnSkillLearned);

            // Inventory events
            eventManager.Subscribe<ItemUsedEvent>(OnItemUsed);
            eventManager.Subscribe<ItemAddedEvent>(OnItemAdded);

            _eventsSubscribed = true;
        }

        /// <summary>
        /// Unsubscribe from all events (call when disposing)
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            if (!_eventsSubscribed) return;

            var eventManager = Core.GameServices.Events;
            if (eventManager == null) return;

            eventManager.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
            eventManager.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            eventManager.Unsubscribe<HealingAppliedEvent>(OnHealingApplied);
            eventManager.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            eventManager.Unsubscribe<LevelUpEvent>(OnLevelUp);
            eventManager.Unsubscribe<SkillLearnedEvent>(OnSkillLearned);
            eventManager.Unsubscribe<ItemUsedEvent>(OnItemUsed);
            eventManager.Unsubscribe<ItemAddedEvent>(OnItemAdded);

            _eventsSubscribed = false;
        }

        #region Event Handlers

        private void OnPartyChanged(PartyChangedEvent evt)
        {
            RefreshPartyStatus();
            _messageLog.AddSystemMessage("Party composition changed.");
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            string message = evt.IsCritical
                ? $"Critical hit! {evt.Amount} damage!"
                : $"{evt.Amount} damage dealt.";
            _messageLog.AddDamageMessage(message);
        }

        private void OnHealingApplied(HealingAppliedEvent evt)
        {
            _messageLog.AddHealMessage($"{evt.Amount} HP restored.");
            RefreshPartyStatus();
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            _messageLog.AddCombatMessage($"Battle started! {evt.Enemies.Count} enemies!");
        }

        private void OnLevelUp(LevelUpEvent evt)
        {
            _messageLog.AddSystemMessage($"Level up! Now level {evt.NewLevel}!");
            RefreshPartyStatus();
        }

        private void OnSkillLearned(SkillLearnedEvent evt)
        {
            _messageLog.AddSystemMessage($"New skill learned: {evt.SkillId}!");
        }

        private void OnItemUsed(ItemUsedEvent evt)
        {
            _messageLog.AddSystemMessage($"Used {evt.ItemId}.");
            RefreshPartyStatus();
        }

        private void OnItemAdded(ItemAddedEvent evt)
        {
            _messageLog.AddSystemMessage($"Obtained {evt.ItemId} x{evt.Quantity}.");
        }

        #endregion

        /// <summary>
        /// Refresh party status display from current game data
        /// </summary>
        public void RefreshPartyStatus()
        {
            var party = Core.GameServices.GameData?.Party;
            if (party == null) return;

            var activeParty = party.ActiveParty;
            _partyStatusHud.ClearAll();

            for (int i = 0; i < activeParty.Count; i++)
            {
                var character = activeParty[i];
                var stats = character.GetComponent<StatsComponent>();
                if (stats != null)
                {
                    _partyStatusHud.UpdateMember(
                        i,
                        character.Name,
                        stats.CurrentHP,
                        stats.MaxHP,
                        stats.CurrentMP,
                        stats.MaxMP
                    );
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_isVisible) return;

            foreach (var element in _hudElements)
            {
                if (element.Visible)
                {
                    element.Update(gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!_isVisible) return;

            var theme = Core.GameServices.UI?.Theme;
            if (theme == null)
            {
                Console.WriteLine("[HUDManager] ERROR: Theme is null");
                return;
            }

            if (theme.DefaultFont == null)
            {
                Console.WriteLine("[HUDManager] ERROR: Theme.DefaultFont is null");
                return;
            }

            foreach (var element in _hudElements)
            {
                if (element.Visible)
                {
                    try
                    {
                        element.Draw(spriteBatch, theme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[HUDManager] ERROR drawing element {element.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        public void ShowPartyStatus(bool show)
        {
            _partyStatusHud.Visible = show;
        }

        public void ShowMessageLog(bool show)
        {
            _messageLog.Visible = show;
        }

        public void HideAll()
        {
            IsVisible = false;
        }

        public void ShowAll()
        {
            IsVisible = true;
        }

        /// <summary>
        /// Hide the HUD (alias for HideAll)
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
        }

        /// <summary>
        /// Show the HUD (alias for ShowAll)
        /// </summary>
        public void Show()
        {
            IsVisible = true;
        }
    }
}
