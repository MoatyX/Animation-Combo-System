using Generics.Utilities;
using UnityEditorInternal;
using UnityEngine;

public class DemoComboSystem : MonoBehaviour
{
    public ComboSequencer ArmedSequencer;
    public ComboSequencer UnarmedSequencer;

    private PlayerController _player;

    private void Start()
    {
        _player = GetComponent<PlayerController>();

        ArmedSequencer.Initialise();
        UnarmedSequencer.Initialise();
    }

    private void Update()
    {
        if (_player.AxeEquiped) ArmedSequencer.Update();
        else UnarmedSequencer.Update();
    }
}
