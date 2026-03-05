using System.Collections.Generic;
using UnityEngine;

public class JarContentsTracker : MonoBehaviour
{
    [SerializeField] private Collider jarInteriorTrigger;

    private readonly HashSet<MarbleColor> inside = new HashSet<MarbleColor>();

    public IReadOnlyCollection<MarbleColor> Inside => inside;

    private void Reset()
    {
        jarInteriorTrigger = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (jarInteriorTrigger == null) jarInteriorTrigger = GetComponent<Collider>();
        if (jarInteriorTrigger != null && !jarInteriorTrigger.isTrigger)
            Debug.LogWarning("[JarContentsTracker] jarInteriorTrigger muss IsTrigger=true sein.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        var mc = other.GetComponentInParent<MarbleColor>();
        if (mc != null) inside.Add(mc);
    }

    private void OnTriggerExit(Collider other)
    {
        var mc = other.GetComponentInParent<MarbleColor>();
        if (mc != null) inside.Remove(mc);
    }

    public void CleanupNulls()
    {
        inside.RemoveWhere(m => m == null);
    }
}