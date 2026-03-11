using UnityEngine;
using Oculus.Interaction; // for OVRHand

public class ControllerJarTrackerCalibrated : MonoBehaviour
{
    [Header("Tracking Anchors")]
    public Transform HandAnchorDetached;
    public Transform ControllerAnchor;

    [Header("Calibration")]
    public bool autoCalibrate = true;
    public float calibrationDelay = 2.0f;
    public bool calibrateNow = false;

    [Header("Smoothing")]
    public float positionSmooth = 25f;
    public float rotationSmooth = 25f;

    [Header("Position Matching")]
    [Tooltip("Maximum allowed distance from previous position to accept new target")]
    public float maxDelta = 0.15f; 

    [Header("Startup Options")]
    [Tooltip("During this time (seconds) always use DetachedAnchor")]
    public float startupGraceSeconds = 2f;

    private Quaternion rotationOffset;
    private float startTime;
    private bool calibrated = false;
    private Vector3 previousPosition;

    void Start()
    {
        startTime = Time.time;
        previousPosition = transform.position;
    }

    void Update()
    {
        Transform targetAnchor = GetValidTargetAnchor();
        if (targetAnchor == null)
            return;

        HandleCalibration(targetAnchor);
        FollowTarget(targetAnchor);
    }

    // Decide which anchor to use based on position matching
    Transform GetValidTargetAnchor()
    {
        float elapsed = Time.time - startTime;
        Vector3 handPos = HandAnchorDetached != null ? HandAnchorDetached.position : Vector3.zero;
        Vector3 controllerPos = ControllerAnchor != null ? ControllerAnchor.position : Vector3.zero;

        // Startup grace period: always use hand anchor if available 
        if (elapsed <= startupGraceSeconds)
            return HandAnchorDetached;

        // Hand anchor valid & roughly matching previous position?
        if (HandAnchorDetached != null)
        {
            if ((handPos - previousPosition).sqrMagnitude <= maxDelta * maxDelta)
                return HandAnchorDetached;
        }

        // Controller roughly matching previous position?
        if (ControllerAnchor != null)
        {
            if ((controllerPos - previousPosition).sqrMagnitude <= maxDelta * maxDelta)
                return ControllerAnchor;
        }

        // neither roughly matching -> ignore position update
        return null;
    }

    void HandleCalibration(Transform target)
    {
        if (!calibrated && autoCalibrate && Time.time - startTime > calibrationDelay)
            Calibrate(target);

        if (calibrateNow)
        {
            calibrateNow = false;
            Calibrate(target);
        }
    }

    public void Calibrate(Transform target)
    {
        Quaternion targetRot = target.rotation;
        Vector3 forwardProjected = Vector3.ProjectOnPlane(targetRot * Vector3.forward, Vector3.up);
        if (forwardProjected.sqrMagnitude < 0.001f)
            forwardProjected = Vector3.forward;

        Quaternion desiredJarRot = Quaternion.LookRotation(forwardProjected, Vector3.up);
        rotationOffset = Quaternion.Inverse(targetRot) * desiredJarRot;
        calibrated = true;

        Debug.Log("Jar calibration completed.");
    }

    void FollowTarget(Transform target)
    {
        // --- Smooth rotation ---
        Quaternion targetRotation = target.rotation * rotationOffset;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);

        // --- Smooth position ---
        Vector3 targetPosition = target.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmooth);

        previousPosition = transform.position;
    }
}