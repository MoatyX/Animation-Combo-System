using UnityEngine;

namespace Generics.Utilities
{
    public class ComboSystem : MonoBehaviour
    {
        public ComboSequencer Sequencer;

        protected virtual void Start()
        {
            Sequencer.Initialise();
        }

        protected virtual void Update()
        {
            Sequencer.Update();
        }
    }
}

