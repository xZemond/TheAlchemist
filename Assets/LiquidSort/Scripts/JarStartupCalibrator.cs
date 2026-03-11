using System.Collections;
using UnityEngine;

public class JarCalibrateFromController : MonoBehaviour
{
    [SerializeField] private Transform controllerAnchor;

    [Header("Startup")]
    [SerializeField] private bool calibrateOnStartup = true;
    [SerializeField] private float startupDelay = 0.5f;

    [Header("Position")]
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;
    [SerializeField] private float lowerYAmount = 0.0f;

    private void Start()
    {
        if (calibrateOnStartup)
            StartCoroutine(CalibrateAfterDelay());
    }

    private IEnumerator CalibrateAfterDelay()
    {
        yield return new WaitForSeconds(startupDelay);
        CalibrateJarUpright();
    }

    [ContextMenu("Calibrate Jar Upright")]
    public void CalibrateJarUpright()
    {
        if (controllerAnchor == null)
            controllerAnchor = transform.parent;

        if (controllerAnchor == null)
        {
            Debug.LogWarning("JarCalibrateFromController: No controller anchor assigned.");
            return;
        }


        // Keep controller yaw, but remove pitch/roll so the jar stands upright
        Vector3 flatForward = Vector3.ProjectOnPlane(controllerAnchor.forward, Vector3.up);

        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.ProjectOnPlane(controllerAnchor.right, Vector3.up);

        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.forward;

        flatForward.Normalize();

        Quaternion targetWorldRotation = Quaternion.LookRotation(flatForward, Vector3.up);

        // Store the corrected local offset relative to the controller anchor
        transform.localRotation = Quaternion.Inverse(controllerAnchor.rotation) * targetWorldRotation;

        
        // Move the jar slightly down into the hand/controller
        Vector3 calibratedLocalPosition = localPositionOffset;
        calibratedLocalPosition.y -= lowerYAmount;
        transform.localPosition = calibratedLocalPosition;
    }
}