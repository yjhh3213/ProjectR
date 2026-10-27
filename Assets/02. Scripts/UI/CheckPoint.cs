using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    public int index;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CheckPoint] Trigger Enter by {other.name}");
        /*if (!other.CompareTag("Player")) return;
        if (lapTimer == null) return;

        lapTimer.OnCheckpointPassed(index);*/
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[CheckPoint] Player detected! Sending index {index}");
        }
    }
}
