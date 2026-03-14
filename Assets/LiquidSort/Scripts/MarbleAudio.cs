using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MarbleAudio : MonoBehaviour
{
    [Header("Audio Clips (Add your .wav files here)")]
    public AudioClip[] glassSounds; // Jars and other marbles
    public AudioClip[] woodSounds;  // Table
    public AudioClip[] stoneSounds; // Floor

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
        audioSource.spatialBlend = 1.0f; // Forces 3D sound just in case!
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextPlayTime) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minimumVelocity) return;

        AudioClip[] selectedSounds = null;

        // Determine what material we just hit
        if (collision.gameObject.CompareTag("Glass") || collision.gameObject.CompareTag("Marble"))
        {
            selectedSounds = glassSounds;
        }
        else if (collision.gameObject.CompareTag("Wood"))
        {
            selectedSounds = woodSounds;
        }
        else if (collision.gameObject.CompareTag("Stone"))
        {
            selectedSounds = stoneSounds;
        }

        // Play a random sound from the correct material list
        if (selectedSounds != null && selectedSounds.Length > 0)
        {
            float volume = Mathf.InverseLerp(minimumVelocity, maxVolumeVelocity, impactSpeed);
            AudioClip randomClip = selectedSounds[Random.Range(0, selectedSounds.Length)];
            
            audioSource.pitch = Random.Range(0.85f, 1.15f); // Randomize pitch
            audioSource.PlayOneShot(randomClip, volume);
            
            nextPlayTime = Time.time + cooldown;
        }
    }
}