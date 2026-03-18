using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JarTeleportManager : MonoBehaviour
{
    [Header("Hand References")]
    public OVRHand rightHand;
    public OVRHand leftHand;
    public float snapThreshold = 0.7f;

    [Header("Startup Delay")]
    public float startupDelay = 2f; // delay before teleporting is enabled
    private bool isReady = false;

    [Header("Jar References")]
    public List<JarContentsTracker> shelfJars;
    public JarContentsTracker leftHandJar;
    public JarContentsTracker rightHandJar;

    public GameObject shelfEffectPrefab;
    public float shelfEffectSize = 0.1f;
    public Transform shelfEffectPosition;

    public GameObject trackedJarEffectPrefab;
    public float trackedEffectSize = 0.05f;

    public float effectLifetime = 3f;

    private bool canTrigger = true;

    private Dictionary<GameObject, Vector3> teleportQueue = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        StartCoroutine(StartupDelayRoutine());
    }

    IEnumerator StartupDelayRoutine()
    {
        yield return new WaitForSeconds(startupDelay);
        isReady = true;
    }

    void Update()
    {
        if (!isReady)
            return;

        bool rightSnap = IsFingerSnap(rightHand);
        bool leftSnap = IsFingerSnap(leftHand);

        if (rightSnap && leftSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;

                SpawnTeleportEffect(shelfEffectPosition.position, shelfEffectPrefab, shelfEffectSize);

                QueueShuffleAllShelves();
                ExecuteTeleports();
            }
        }
        else if (rightSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;

                Vector3 effectPos = leftHandJar.transform.position;
                effectPos.y -= 0.1f;

                SpawnTeleportEffect(effectPos, trackedJarEffectPrefab, trackedEffectSize);
                SpawnTeleportEffect(shelfEffectPosition.position, shelfEffectPrefab, shelfEffectSize);

                QueueSnapRightHand();
                ExecuteTeleports();
            }
        }
        else if (leftSnap)
        {
            if (canTrigger)
            {
                canTrigger = false;

                Vector3 effectPos = rightHandJar.transform.position;
                effectPos.y -= 0.1f;

                SpawnTeleportEffect(effectPos, trackedJarEffectPrefab, trackedEffectSize);
                SpawnTeleportEffect(shelfEffectPosition.position, shelfEffectPrefab, shelfEffectSize);

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
        QueueTeleportContents(shelfJars[0], leftHandJar);
        QueueTeleportContents(leftHandJar, shelfJars[shelfJars.Count - 1]);

        for (int i = 1; i < shelfJars.Count; i++)
        {
            QueueTeleportContents(shelfJars[i], shelfJars[i - 1]);
        }
    }

    void QueueSnapLeftHand()
    {
        int lastIndex = shelfJars.Count - 1;

        QueueTeleportContents(shelfJars[lastIndex], rightHandJar);
        QueueTeleportContents(rightHandJar, shelfJars[0]);

        for (int i = 0; i < lastIndex; i++)
        {
            QueueTeleportContents(shelfJars[i], shelfJars[i + 1]);
        }
    }

    void QueueShuffleAllShelves()
    {
        int n = shelfJars.Count;
        List<JarContentsTracker> shuffled = new List<JarContentsTracker>(shelfJars);

        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

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
            Vector3 offset = m.transform.position - fromCenter;
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
        yield return new WaitForFixedUpdate();

        foreach (var rb in bodies)
        {
            if (rb != null)
                rb.isKinematic = false;
        }
    }

    void SpawnTeleportEffect(Vector3 position, GameObject prefab, float scale)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        Destroy(effect, effectLifetime);
    }
}