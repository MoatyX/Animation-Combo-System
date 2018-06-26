using Generics.Utilities;
using UnityEngine;

public class DemoComboSystem : MonoBehaviour
{
    [Header("Sequencers")]
    public ComboSequencer ArmedSequencer;
    public ComboSequencer UnarmedSequencer;

    [Header("Simple API Examples")]
    public bool IsExecutingCombo;
    public Combo ActiveCombo;

    private PlayerController _player;

    private void Start()
    {
        _player = GetComponent<PlayerController>();

        //must be called once at the beginning to setup and initialise the sequencers
        ArmedSequencer.Initialise();
        UnarmedSequencer.Initialise();
    }

    private void Update()
    {
        if (_player.AxeEquiped)
        {
            //must be called to actually listen-to and buffer input/anims and executing them
            ArmedSequencer.Update();

            //example simple API. use this for your own custom logic
            IsExecutingCombo = ArmedSequencer.IsExecuting();
            ActiveCombo = ArmedSequencer.ActiveCombo;
        }
        else
        {
            UnarmedSequencer.Update();
            IsExecutingCombo = UnarmedSequencer.IsExecuting();
            ActiveCombo = UnarmedSequencer.ActiveCombo;
        }

    }
}
