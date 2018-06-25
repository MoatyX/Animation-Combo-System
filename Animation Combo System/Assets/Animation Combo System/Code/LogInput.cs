using UnityEngine;

public class LogInput : MonoBehaviour
{
    void Update()
    {
        if(string.IsNullOrEmpty(Input.inputString)) return;

        Debug.Log(Input.inputString);
    }
}
