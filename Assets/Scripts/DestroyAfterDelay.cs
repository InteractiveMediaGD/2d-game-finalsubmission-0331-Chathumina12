using UnityEngine;

/// <summary>
/// Simple utility script to automatically destroy a GameObject after a set delay.
/// Perfect for fire-and-forget Particle Systems.
/// </summary>
public class DestroyAfterDelay : MonoBehaviour 
{ 
    [Tooltip("Seconds before destruction")]
    public float delay = 2f; 
    
    void Start() 
    { 
        Destroy(gameObject, delay); 
    } 
}
