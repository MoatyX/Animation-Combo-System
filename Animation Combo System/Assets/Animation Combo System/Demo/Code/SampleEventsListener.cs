using Generics.Utilities;
using UnityEngine;

/// <summary>
/// a simple Damage handler code
/// </summary>
public class SampleEventsListener : MonoBehaviour
{
    [Header("Hit Scanning")]
    public Transform ScanPoint;
    public float RayLength;
    public LayerMask LayerMask;

    private void OnEnable()
    {
        Dispatcher.HitScanning += OnHitScanning;
        Dispatcher.ComboCompleted += OnComboCompleted;
        Dispatcher.AttackTriggered += OnAttackTriggered;
    }

    private void OnDisable()
    {
        Dispatcher.HitScanning -= OnHitScanning;
    }

    /// <summary>
    /// a custom logic on hit scanning
    /// </summary>
    private void OnHitScanning(AttackAnim attack)
    {
        //TODO: your custom hit detection here (doesnt have to be raycasting, could be anything depending on your game)
        //TODO: your custom audio playing here (sword hitting the enemy's armor ?)
        //TODO: your custom damage handling logic here
        //TODO: plus any other custom logic and sequences here

        Debug.Log("Scanning");

        Ray ray = new Ray(transform.position, transform.forward);
        bool hit = Physics.Raycast(ray, RayLength, LayerMask, QueryTriggerInteraction.Ignore);
        if(!hit) return;

        Debug.Log("We have hit something....Apply damage, play audio...etc !");
    }

    private void OnComboCompleted(Combo combo)
    {
        Debug.Log("the combo was compeleted");
    }

    private void OnAttackTriggered(AttackAnim attack)
    {
        Debug.Log("the attack: " + attack.AnimName + ", was triggered");
    }
}
