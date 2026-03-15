using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoopingEffect : MonoBehaviour
{
    public GameObject effectPrefab; // Assign your particle prefab in the inspector
    private GameObject effectInstance;

    void Start()
    {
        // Instantiate and parent to this object
        effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
        // effectInstance.loop = true; // Make sure the system loops
        // effectInstance.Play();
    }
}