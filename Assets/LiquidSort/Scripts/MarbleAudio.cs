using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MarbleAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] impactSounds;

    [Header("Collision Settings")]
    public float minimumVelocity = 0.5f;
    public float maxVolumeVelocity = 5.0f;

    [Header("Spam Prevention")]
    public float cooldown = 0.05f;

    private AudioSource audioSource;
    private float nextPlayTime = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Makes it 3D sound in VR
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextPlayTime || impactSounds.Length == 0) return;

        // NEW: Check if it hits the Jar ("Glass") OR another Marble ("Marble")
        if (collision.gameObject.CompareTag("Glass") || collision.gameObject.CompareTag("Marble"))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed > minimumVelocity)
            {
                float volume = Mathf.InverseLerp(minimumVelocity, maxVolumeVelocity, impactSpeed);
                AudioClip randomClip = impactSounds[Random.Range(0, impactSounds.Length)];
                
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(randomClip, volume);
                
                nextPlayTime = Time.time + cooldown;
            }
        }
    }
}