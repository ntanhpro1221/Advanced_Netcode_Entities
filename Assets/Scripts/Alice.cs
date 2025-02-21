using UnityEngine;

public class Alice : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        Debug.Log($"{name} trigger {other.name}");
    }

    private void OnCollisionEnter(Collision other) {
        Debug.Log($"{name} collision {other.collider.name}");
    }
}   
