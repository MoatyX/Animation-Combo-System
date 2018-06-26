namespace Generics.Utilities
{
    public enum SequenceType
    {
        /// <summary>
        /// the input must be fully completed before any anim/combo would trigger
        /// </summary>
        Full,

        /// <summary>
        /// trigger the next anim in the combo when a correct key stoke is inserted at the correct-predetermained timing
        /// </summary>
        Partial,

        /// <summary>
        /// Ignore the predetermained timing and trigger the next anim in the combo as long as the key strokes are correct
        /// </summary>
        PartialAppending
    }
}