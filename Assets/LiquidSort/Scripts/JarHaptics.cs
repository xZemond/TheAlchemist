using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required to keep track of marbles

public class JarHaptics : MonoBehaviour
{
    [Header("Meta Quest Haptic Settings")]
    [Range(0f, 1f)] public float hitAmplitude = 0.8f; 
    [Range(0f, 1f)] public float hitFrequency = 0.5f; 
    public float hitDuration = 0.05f;                 
    public float cooldown = 0.15f;                    

    private float nextHapticTime = 0f;
    
    // This list remembers which marbles are already inside
    private HashSet<Collider> marblesInside = new HashSet<Collider>();

    private void Start()
    {
        if (gameObject.CompareTag("Marble"))
        {
            Destroy(this);
        }
    }

    // Trigger when a marble first ENTERS the jar
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Marble"))
        {
            // If we haven't seen this marble before...
            if (!marblesInside.Contains(other))
            {
                // Add it to our memory list
                marblesInside.Add(other);
                
                // Vibrate the controllers!
                TryVibrateBoth();
            }
        }
    }

    // Trigger when a marble leaves the jar
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Marble"))
        {
            // Remove it from memory so it can cause a vibration if it falls back in
            if (marblesInside.Contains(other))
            {
                marblesInside.Remove(other);
            }
        }
    }

    private void TryVibrateBoth()
    {
        if (Time.time < nextHapticTime) return;
        nextHapticTime = Time.time + cooldown;
        
        Debug.Log("Marble ENTERED Jar! Single Pulse Haptics sent.");
        StartCoroutine(TriggerOVRHaptics());
    }

    private IEnumerator TriggerOVRHaptics()
    {
        OVRInput.SetControllerVibration(hitFrequency, hitAmplitude, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(hitFrequency, hitAmplitude, OVRInput.Controller.LTouch);
        
        yield return new WaitForSeconds(hitDuration);
        
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }
}