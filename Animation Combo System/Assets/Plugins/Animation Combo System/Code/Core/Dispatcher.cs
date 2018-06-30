using UnityEngine;

namespace Generics.Utilities
{
    /// <summary>
    /// a simple event system
    /// </summary>
    public static class Dispatcher
    {
        //Note: system is still under-construction

        public delegate void HitScanHandler(AttackAnim attack, uint scanIndex, float damage);
        public delegate void GenericEventHandler(AttackAnim attack, uint index, string key);
        public delegate void AttackTriggeredHandler(AttackAnim attack);
        public delegate void ComboCompletedHandler(Combo combo);

        /// <summary>
        /// an event sent at a specific timing for functions to handle the logic/to extend the hit scanning
        /// </summary>
        public static event HitScanHandler HitScanning;

        /// <summary>
        /// an event triggered at a specified timing for functions that want to implement a custom logic
        /// </summary>
        public static event GenericEventHandler GenericEvent;

        /// <summary>
        /// an event triggered when an attack is triggered
        /// </summary>
        public static event AttackTriggeredHandler AttackTriggered;

        /// <summary>
        /// an event triggered when a combo has been successfully compeleted
        /// </summary>
        public static event ComboCompletedHandler ComboCompleted;



        internal static void OnHitScanning(AttackAnim attack, uint scanIndex, float damage)
        {
            var handler = HitScanning;
            if (handler != null) handler(attack, scanIndex, damage);
        }

        internal static void OnAttackTriggered(AttackAnim attack)
        {
            var handler = AttackTriggered;
            if (handler != null) handler(attack);
        }

        internal static void OnComboCompleted(Combo combo)
        {
            var handler = ComboCompleted;
            if (handler != null) handler(combo);
        }

        internal static void OnGenericEvent(AttackAnim attack, uint index, string key)
        {
            var handler = GenericEvent;
            if (handler != null) handler(attack, index, key);
        }
    }
}