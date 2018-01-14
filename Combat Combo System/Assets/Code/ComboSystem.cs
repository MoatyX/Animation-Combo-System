using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public ComboSequencer combo;
    public ComboSequencer combo2;

    void Start()
    {
        combo.Setup();
        combo2.Setup();
    }

    void Update ()
    {
		combo.Update();
        combo2.Update();
	}
}
