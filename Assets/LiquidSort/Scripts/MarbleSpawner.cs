using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MarbleBatch
    {
        public string name = "Color";
        public int count = 25;
        public Material material;
        [Tooltip("Pause nach diesem Farb-Batch (Sekunden). Wird als Settling-Fenster genutzt.")]
        public float delayAfterBatch = 1.0f;
        public int colorId;
    }

    [Header("References")]
    [SerializeField] private GameObject marblePrefab;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Transform marbleParent;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Batches")]
    [SerializeField] private List<MarbleBatch> batches = new List<MarbleBatch>();

    private readonly List<GameObject> spawnedMarbles = new List<GameObject>();
    private Coroutine spawnRoutine;

    private void Start()
    {
        if (marblePrefab == null)
        {
            Debug.LogError("[MarbleSpawner] marblePrefab not assigned.", this);
            return;
        }
        if (spawnArea == null)
        {
            Debug.LogError("[MarbleSpawner] spawnArea not assigned.", this);
            return;
        }

        if (spawnOnStart)
            StartCoroutine(SpawnAfterDelay());
    }

    IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(2);
        SpawnMarbles();
    }

    [ContextMenu("Spawn Marbles")]
    public void SpawnMarbles()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int b = 0; b < batches.Count; b++)
        {
            MarbleBatch batch = batches[b];
            if (batch == null || batch.count <= 0) continue;

            // Keep track of this batch's marbles
            List<GameObject> currentBatchMarbles = new List<GameObject>();

            // Spawn this batch
            for (int i = 0; i < batch.count; i++)
            {
                Vector3 pos = GetRandomPointInBox(spawnArea);
                GameObject marble = Instantiate(marblePrefab, pos, Random.rotation, marbleParent);
                spawnedMarbles.Add(marble);
                currentBatchMarbles.Add(marble);

                var mc = marble.GetComponent<MarbleColor>();
                if (mc != null) mc.colorId = batch.colorId;

                // Set material
                ApplyMaterialShared(marble, batch.material);

                if (spawnInterval > 0f)
                    yield return new WaitForSeconds(spawnInterval);
                else
                    yield return null;
            }

            // Pause between layers
            float interval = Mathf.Max(0f, batch.delayAfterBatch);
            if (interval > 0f)
                yield return new WaitForSeconds(interval);

            // Disable physics after the interval
            foreach (var marble in currentBatchMarbles)
            {
                var rb = marble.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
            }
        }

        spawnRoutine = null;
    }


    private void CleanupSpawnedList()
    {
        spawnedMarbles.RemoveAll(go => go == null);
    }

    private void ApplyMaterialShared(GameObject marble, Material mat)
    {
        if (mat == null) return;

        var renderers = marble.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.sharedMaterial = mat; // besser als r.material für viele Instanzen
    }

    private Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);

        return center + new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f),
            Random.Range(-size.z * 0.5f, size.z * 0.5f)
        );
    }

    [ContextMenu("Clear Marbles")]
    public void ClearMarbles()
    {
        for (int i = spawnedMarbles.Count - 1; i >= 0; i--)
            if (spawnedMarbles[i] != null) Destroy(spawnedMarbles[i]);

        spawnedMarbles.Clear();
    }
}