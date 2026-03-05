using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MarbleSettler : MonoBehaviour
{
    [Header("Calm detection")]
    [SerializeField] private float linearSpeedToBeCalm = 0.05f;
    [SerializeField] private float angularSpeedToBeCalm = 0.5f;
    [SerializeField] private float requiredCalmTime = 0.75f;

    [Header("Soft lock values")]
    [SerializeField] private float normalDrag = 0.05f;
    [SerializeField] private float normalAngularDrag = 0.2f;

    [SerializeField] private float softLockDrag = 8f;
    [SerializeField] private float softLockAngularDrag = 10f;

    [SerializeField] private float velocityDamping = 8f;
    [SerializeField] private float angularDamping = 10f;

    [Header("Unlock thresholds")]
    [SerializeField] private float unlockLinearSpeed = 0.2f;
    [SerializeField] private float unlockAngularSpeed = 2.0f;

    private Rigidbody rb;
    private float calmTimer;
    private bool softLocked;
    private bool armed = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ApplyNormalState();
    }

    private void FixedUpdate()
    {
        if (!armed)
            return;

        float linearSpeed = rb.linearVelocity.magnitude;
        float angularSpeed = rb.angularVelocity.magnitude;

        if (!softLocked)
        {
            bool calm =
                linearSpeed < linearSpeedToBeCalm &&
                angularSpeed < angularSpeedToBeCalm;

            if (calm)
                calmTimer += Time.fixedDeltaTime;
            else
                calmTimer = 0f;

            if (calmTimer >= requiredCalmTime)
                SoftLock();
        }
        else
        {
            // aktive Dämpfung im Soft-Lock
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                Vector3.zero,
                velocityDamping * Time.fixedDeltaTime
            );

            rb.angularVelocity = Vector3.Lerp(
                rb.angularVelocity,
                Vector3.zero,
                angularDamping * Time.fixedDeltaTime
            );

            bool shouldUnlock =
                linearSpeed > unlockLinearSpeed ||
                angularSpeed > unlockAngularSpeed;

            if (shouldUnlock)
                Unlock();
        }
    }

    public void SoftLock()
    {
        softLocked = true;
        calmTimer = 0f;

        rb.linearDamping = softLockDrag;
        rb.angularDamping = softLockAngularDrag;
        rb.Sleep();
    }

    public void Unlock()
    {
        softLocked = false;
        calmTimer = 0f;
        ApplyNormalState();
        rb.WakeUp();
    }

    public void ArmForInterval(float intervalSeconds)
    {
        requiredCalmTime = Mathf.Clamp(intervalSeconds * 0.7f, 0.2f, 1.5f);
        calmTimer = 0f;
        armed = true;
    }

    public void Disarm()
    {
        calmTimer = 0f;
        armed = false;
    }

    private void ApplyNormalState()
    {
        rb.linearDamping = normalDrag;
        rb.angularDamping  = normalAngularDrag;
    }

    public bool IsSoftLocked => softLocked;
}