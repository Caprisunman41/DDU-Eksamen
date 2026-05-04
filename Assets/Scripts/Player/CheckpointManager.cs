using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    public Vector3 RespawnPosition { get; private set; }

    private Checkpoint _activeCheckpoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetCheckpoint(Vector3 position, Checkpoint checkpoint)
    {
        if (_activeCheckpoint != null && _activeCheckpoint != checkpoint)
            _activeCheckpoint.Deactivate();

        _activeCheckpoint = checkpoint;
        RespawnPosition = position;
    }
}
