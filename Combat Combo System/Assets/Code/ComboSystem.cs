using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public ComboSequencer combo;

    void Start()
    {
        combo.Setup();
    }

    void Update ()
    {
		combo.Update();
	}
}
