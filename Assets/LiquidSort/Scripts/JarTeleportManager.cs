using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Collections.Generic;
public class JarTeleportManager : MonoBehaviour
{
    [Header("Hand References")]
    public OVRHand rightHand;
    public OVRHand leftHand;
    public float snapThreshold = 0.7f;

    [Header("Jar References")]
    public List<JarContentsTracker> shelfJars;   // Ordered left to right
    public JarContentsTracker leftHandJar;
    public JarContentsTracker rightHandJar;

    private bool canTrigger = true;

    // Stores the marbles and their final positions
    private Dictionary<GameObject, Vector3> teleportQueue = new Dictionary<GameObject, Vector3>();

    void Update()
    {
        bool rightSnap = IsFingerSnap(rightHand);
        bool leftSnap = IsFingerSnap(leftHand);

        if (rightSnap && leftSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;
                QueueShuffleAllShelves();
                ExecuteTeleports();
            }
        }
        else if (rightSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;
                QueueSnapRightHand();
                ExecuteTeleports();
            }
        }
        else if (leftSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;
                QueueSnapLeftHand();
                ExecuteTeleports();
            }
        }
        else
        {
            canTrigger = true;
        }
    }

    bool IsFingerSnap(OVRHand hand)
    {
        if (hand == null) return false;

        float indexStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float middleStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);

        return indexStrength > snapThreshold && middleStrength > snapThreshold;
    }

    // ---------------- Queue Functions ---------------- //

    void QueueSnapRightHand()
    {
        // Most left shelf -> left hand
        QueueTeleportContents(shelfJars[0], leftHandJar);

        // Left hand -> most right shelf
        QueueTeleportContents(leftHandJar, shelfJars[shelfJars.Count - 1]);

        // Other shelf jars -> next jar to the left
        for (int i = 1; i < shelfJars.Count; i++)
        {
            QueueTeleportContents(shelfJars[i], shelfJars[i - 1]);
        }
    }

    void QueueSnapLeftHand()
    {
        int lastIndex = shelfJars.Count - 1;

        // Most right shelf -> right hand
        QueueTeleportContents(shelfJars[lastIndex], rightHandJar);

        // Right hand -> most left shelf
        QueueTeleportContents(rightHandJar, shelfJars[0]);

        // Other shelf jars -> next jar to the right
        for (int i = 0; i < lastIndex; i++)
        {
            QueueTeleportContents(shelfJars[i], shelfJars[i + 1]);
        }
    }

    void QueueShuffleAllShelves()
    {
        int n = shelfJars.Count;

        // Create shuffled copy
        List<JarContentsTracker> shuffled = new List<JarContentsTracker>(shelfJars);

        // Fisher–Yates shuffle
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Create 1-to-1 mapping
        for (int i = 0; i < n; i++)
        {
            QueueTeleportContents(shelfJars[i], shuffled[i]);
        }
    }

    void QueueTeleportContents(JarContentsTracker fromJar, JarContentsTracker toJar)
    {
        List<GameObject> marbles = fromJar.GetAllMarbles();
        if (marbles.Count == 0) return;

        Vector3 fromCenter = fromJar.MarbleContentsCollider.bounds.center;
        Vector3 toCenter = toJar.MarbleContentsCollider.bounds.center;

        foreach (var m in marbles)
        {
            SetKinematic(m, true);

            // Compute offset from the marble’s original position
            Vector3 offset = m.transform.position - fromCenter;

            // Store the final target position in the queue
            teleportQueue[m] = toCenter + offset;
        }
    }

    void ExecuteTeleports()
    {
        List<Rigidbody> bodies = new List<Rigidbody>();

        foreach (var kvp in teleportQueue)
        {
            GameObject m = kvp.Key;
            Vector3 target = kvp.Value;

            Rigidbody rb = m.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                bodies.Add(rb);
            }

            m.transform.position = target;
        }

        teleportQueue.Clear();

        StartCoroutine(EnablePhysicsNextFrame(bodies));
    }

    IEnumerator EnablePhysicsNextFrame(List<Rigidbody> bodies)
    {
        yield return new WaitForFixedUpdate(); // wait one physics step

        foreach (var rb in bodies)
        {
            if (rb != null)
                rb.isKinematic = false;
        }
    }

    void SetKinematic(GameObject obj, bool state)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = state;
    }
}