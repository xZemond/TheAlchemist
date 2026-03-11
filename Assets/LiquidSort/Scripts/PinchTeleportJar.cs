using UnityEngine;
using System.Collections.Generic;

public class PinchTeleportJar : MonoBehaviour
{
    public OVRHand rightHand;
    public OVRHand leftHand;

    public float pinchThreshold = 0.7f;

    public JarContentsTracker ownJar;
    public JarContentsTracker trackedJar;

    private bool canTrigger = true;

    void Update()
    {
        if (IsPinching(rightHand) || IsPinching(leftHand))
            TryActivate();
        else
            canTrigger = true;
    }

    bool IsPinching(OVRHand hand)
    {
        if (hand == null) return false;
        return hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinchThreshold;
    }

    void TryActivate()
    {
        if (!canTrigger) return;

        canTrigger = false;
        TeleportSwitch();
    }

    public void TeleportSwitch()
    {
        List<GameObject> ownMarbles = ownJar.GetAllMarbles();
        List<GameObject> trackedMarbles = trackedJar.GetAllMarbles();

        Debug.Log("Own jar marbles: " + ownJar.GetAllMarbles().Count);
        Debug.Log("Tracked jar marbles: " + trackedJar.GetAllMarbles().Count);

        Vector3 ownCenter = ownJar.MarbleContentsCollider.bounds.center;
        Vector3 trackedCenter = trackedJar.MarbleContentsCollider.bounds.center;

        // store relative offsets
        Dictionary<GameObject, Vector3> ownOffsets = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Vector3> trackedOffsets = new Dictionary<GameObject, Vector3>();

        foreach (var m in ownMarbles)
        {
            SetKinematic(m, true);
            ownOffsets[m] = m.transform.position - ownCenter;
        }

        foreach (var m in trackedMarbles)
        {
            SetKinematic(m, true);
            trackedOffsets[m] = m.transform.position - trackedCenter;
        }

        // move marbles
        foreach (var m in ownMarbles)
            m.transform.position = trackedCenter + ownOffsets[m];

        foreach (var m in trackedMarbles)
            m.transform.position = ownCenter + trackedOffsets[m];

        // re-enable physics
        foreach (var m in ownMarbles)
            SetKinematic(m, false);

        foreach (var m in trackedMarbles)
            SetKinematic(m, false);

        Debug.Log("Jar contents swapped");
    }

    void SetKinematic(GameObject obj, bool state)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = state;
    }
}