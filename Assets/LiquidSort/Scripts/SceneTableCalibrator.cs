using UnityEngine;

public class RoomCalibrator : MonoBehaviour
{
    [Header("Roots")]
    public Transform roomRoot;          // Parent of the entire virtual room

    [Header("Tracked Jars")]
    public Transform leftJar;
    public Transform rightJar;

    [Header("Anchor Positions in Room")]
    public Transform leftJarAnchor;
    public Transform rightJarAnchor;

    [Header("Startup")]
    public float calibrationDelay = 2f;

    private void Start()
    {
        Invoke(nameof(RecalibrateRoom), calibrationDelay);
    }

    [ContextMenu("Recalibrate Room")]
    public void RecalibrateRoom()
    {
        if (roomRoot == null ||
            leftJar == null || rightJar == null ||
            leftJarAnchor == null || rightJarAnchor == null)
        {
            Debug.LogWarning("Calibration references missing.");
            return;
        }

        // -------------------------------
        // 1️Compute translation: move anchor center to jar center
        // -------------------------------
        Vector3 anchorCenter = (leftJarAnchor.position + rightJarAnchor.position) * 0.5f;
        Vector3 jarCenter = (leftJar.position + rightJar.position) * 0.5f;
        Vector3 translation = jarCenter - anchorCenter;

        roomRoot.position += translation;

        // -------------------------------
        // 2Compute rotation: align anchor vector to jar vector
        // -------------------------------
        Vector3 anchorDir = rightJarAnchor.position - leftJarAnchor.position;
        Vector3 jarDir = rightJar.position - leftJar.position;

        anchorDir.y = 0f;
        jarDir.y = 0f;

        if (anchorDir.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("Anchors too close together for rotation calculation.");
            return;
        }

        Quaternion rotOffset = Quaternion.FromToRotation(anchorDir, jarDir);

        // Rotate room around anchor center
        roomRoot.RotateAround(anchorCenter, Vector3.up, rotOffset.eulerAngles.y);

        // -------------------------------
        // Reset jar trackers
        // -------------------------------
        ControllerJarTrackerCalibrated[] trackers =
            FindObjectsOfType<ControllerJarTrackerCalibrated>();

        foreach (var t in trackers)
            t.ResetTrackingBaseline();

        Debug.Log("Room recalibrated: anchors aligned to jars.");
    }
}