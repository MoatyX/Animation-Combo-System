using UnityEngine;

namespace Generics.Utilities
{
    /// <summary>
    /// a simple event system
    /// </summary>
    public static class Dispatcher
    {
        //Note: system is still under-construction

        public delegate void HitScanNotification(AttackAnim attack);
        public delegate void AttackTriggeredHandler(AttackAnim attack);
        public delegate void ComboCompletedHandler(Combo combo);

        /// <summary>
        /// an event sent at a specific timing for functions to handle the logic/to extend the hit scanning
        /// </summary>
        public static event HitScanNotification HitScanning;

        /// <summary>
        /// an event triggered when an attack is triggered
        /// </summary>
        public static event AttackTriggeredHandler AttackTriggered;

        /// <summary>
        /// an event triggered when a combo has been successfully compeleted
        /// </summary>
        public static event ComboCompletedHandler ComboCompleted;



        public static void OnHitScanning(AttackAnim attack)
        {
            var handler = HitScanning;
            if (handler != null) handler(attack);
        }

        public static void OnAttackTriggered(AttackAnim attack)
        {
            var handler = AttackTriggered;
            if (handler != null) handler(attack);
        }

        public static void OnComboCompleted(Combo combo)
        {
            var handler = ComboCompleted;
            if (handler != null) handler(combo);
        }
    }
}