using System;
using System.Collections.Generic;
using System.Linq;
using TheMythalProphecy.Game.Characters.Stats;

namespace TheMythalProphecy.Game.Battle
{
    /// <summary>
    /// Manages speed-based turn order queue for battle
    /// </summary>
    public class TurnOrderSystem
    {
        private readonly List<Combatant> _combatants;
        private Queue<Combatant> _turnQueue;
        private int _roundNumber;
        private readonly Random _random;

        public int RoundNumber => _roundNumber;

        public TurnOrderSystem(List<Combatant> combatants)
        {
            _combatants = combatants;
            _turnQueue = new Queue<Combatant>();
            _roundNumber = 1;
            _random = new Random();
        }

        /// <summary>
        /// Calculates initial turn order based on speed
        /// Formula: Priority = (Speed * 10) + Random(0, 50)
        /// </summary>
        public void CalculateInitialTurnOrder()
        {
            // Calculate priority for each combatant
            foreach (var combatant in _combatants.Where(c => c.IsAlive))
            {
                int speed = combatant.Stats.GetStat(StatType.Speed);
                float priority = (speed * 10) + _random.Next(0, 51);
                combatant.TurnPriority = priority;
            }

            // Sort by priority (descending - highest goes first)
            var sortedCombatants = _combatants
                .Where(c => c.IsAlive)
                .OrderByDescending(c => c.TurnPriority)
                .ToList();

            // Build the turn queue
            _turnQueue.Clear();
            foreach (var combatant in sortedCombatants)
            {
                _turnQueue.Enqueue(combatant);
            }

            _roundNumber = 1;
        }

        /// <summary>
        /// Gets the next combatant in turn order
        /// Recalculates turn order if queue is empty (new round)
        /// </summary>
        public Combatant GetNextTurn()
        {
            // If queue is empty, start a new round
            if (_turnQueue.Count == 0)
            {
                _roundNumber++;
                RecalculateTurnOrder();
            }

            // Dequeue next combatant
            if (_turnQueue.Count > 0)
            {
                var nextCombatant = _turnQueue.Dequeue();

                // If the combatant is dead, get the next one
                if (!nextCombatant.IsAlive)
                {
                    return GetNextTurn();
                }

                return nextCombatant;
            }

            return null;
        }

        /// <summary>
        /// Recalculates turn order for a new round
        /// Filters out dead combatants
        /// </summary>
        public void RecalculateTurnOrder()
        {
            // Get only alive combatants
            var aliveCombatants = _combatants.Where(c => c.IsAlive).ToList();

            if (aliveCombatants.Count == 0)
                return;

            // Recalculate priorities with new random values
            foreach (var combatant in aliveCombatants)
            {
                int speed = combatant.Stats.GetStat(StatType.Speed);
                float priority = (speed * 10) + _random.Next(0, 51);
                combatant.TurnPriority = priority;
            }

            // Sort by priority
            var sortedCombatants = aliveCombatants
                .OrderByDescending(c => c.TurnPriority)
                .ToList();

            // Rebuild queue
            _turnQueue.Clear();
            foreach (var combatant in sortedCombatants)
            {
                _turnQueue.Enqueue(combatant);
            }
        }

        /// <summary>
        /// Removes a combatant from the turn queue (when they die)
        /// </summary>
        public void RemoveCombatant(Combatant combatant)
        {
            // Remove from main list
            _combatants.Remove(combatant);

            // Rebuild queue without the dead combatant
            if (_turnQueue.Contains(combatant))
            {
                var remainingCombatants = _turnQueue.Where(c => c != combatant).ToList();
                _turnQueue.Clear();
                foreach (var c in remainingCombatants)
                {
                    _turnQueue.Enqueue(c);
                }
            }
        }

        /// <summary>
        /// Previews the next N combatants in turn order without dequeueing
        /// Useful for UI display
        /// </summary>
        public List<Combatant> PreviewTurnOrder(int count)
        {
            return _turnQueue.Take(count).ToList();
        }

        /// <summary>
        /// Gets the current queue size
        /// </summary>
        public int QueueSize => _turnQueue.Count;

        /// <summary>
        /// Checks if there are any combatants left in the queue
        /// </summary>
        public bool HasNextTurn()
        {
            return _turnQueue.Count > 0 || _combatants.Any(c => c.IsAlive);
        }
    }
}
