using System.Linq;
using System.Collections.Generic;

namespace Generics.Utilities
{
    /// <summary>
    /// Defines 1 Link that makes up the Chain
    /// </summary>
    public class ChainLink
    {
        public Queue<AttackAnim> Combos = new Queue<AttackAnim>();
        public bool HasFinished;

        private readonly List<AttackAnim> _mainCombos = new List<AttackAnim>();  //reference copy

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="combos"></param>
        public ChainLink(IEnumerable<AttackAnim> combos)
        {
            _mainCombos = combos.ToList();

            IEnumerator<AttackAnim> combo = combos.GetEnumerator();
            while (combo.MoveNext())
            {
                this.Combos.Enqueue(combo.Current);
            }

            combo.Dispose();
            HasFinished = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="combo"></param>
        public ChainLink(AttackAnim combo)
        {
            _mainCombos.Add(combo);
            Combos.Enqueue(combo);
            HasFinished = true;
        }

        /// <summary>
        /// Rebuild the link
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            Combos = new Queue<AttackAnim>(_mainCombos);
            HasFinished = true;

            return true;
        }
    }
}