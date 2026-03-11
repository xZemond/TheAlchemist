using UnityEngine;

public class PinchInteractable : MonoBehaviour
{
    public float pinchThreshold = 0.8f;
    public float maxDistance = 3f; // how far pinch can trigger
    public OVRHand leftHand;
    public OVRHand rightHand;
    public LayerMask jarLayer; // assign jars to a dedicated layer

    private bool canTrigger = true;

    void Update()
    {
        CheckHand(rightHand);
        CheckHand(leftHand);
    }

    void CheckHand(OVRHand hand)
    {
        if (hand == null) return;

        // Only proceed if hand is pinching
        if (hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) < pinchThreshold)
        {
            canTrigger = true;
            return;
        }

        // Raycast from hand forward
        Ray ray = new Ray(hand.transform.position, hand.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, jarLayer))
        {
            // Only trigger if pointing at THIS jar
            if (hit.collider.gameObject == gameObject)
            {
                TryActivate();
            }
        }
    }

    void TryActivate()
    {
        if (!canTrigger) return;

        canTrigger = false;

        // Call the jar-specific teleport or other logic
        GetComponent<PinchTeleportJar>()?.TeleportSwitch();
    }
}