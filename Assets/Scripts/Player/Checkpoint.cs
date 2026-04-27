using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float _spawnOffsetY = 1.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            CheckpointManager.Instance.SetCheckpoint(transform.position + Vector3.up * _spawnOffsetY);
    }
}
