using UnityEngine;
using System.Collections.Generic;

public class JarContentsTracker : MonoBehaviour
{
    public Collider marbleContentsCollider;

    public Collider MarbleContentsCollider
    {
        get { return marbleContentsCollider; }
    }

    public List<GameObject> GetAllMarbles()
    {
        if (marbleContentsCollider == null)
        {
            Debug.LogError($"JarContentsTracker on {gameObject.name} has NO collider assigned!");
            return new List<GameObject>();
        }
        List<GameObject> marbles = new List<GameObject>();

        Collider[] hits = Physics.OverlapBox(
            marbleContentsCollider.bounds.center,
            marbleContentsCollider.bounds.extents,
            marbleContentsCollider.transform.rotation
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Marble"))
                marbles.Add(hit.gameObject);
        }

        return marbles;
    }
}