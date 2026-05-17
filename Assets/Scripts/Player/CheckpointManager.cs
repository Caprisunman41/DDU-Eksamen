using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public Vector3 RespawnPosition
    {
        get
        {
            if (GameStateManager.Instance != null
                && GameStateManager.Instance.HasCheckpoint
                && GameStateManager.Instance.LastCheckpointScene == SceneManager.GetActiveScene().name)
            {
                return GameStateManager.Instance.LastCheckpointPos;
            }
            return _fallbackPosition;
        }
    }

    private Vector3 _fallbackPosition;
    private Checkpoint _activeCheckpoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetFallback(Vector3 position)
    {
        _fallbackPosition = position;
    }

    public void SetCheckpoint(Vector3 position, Checkpoint checkpoint)
    {
        if (_activeCheckpoint != null && _activeCheckpoint != checkpoint)
            _activeCheckpoint.Deactivate();

        _activeCheckpoint = checkpoint;
        _fallbackPosition = position;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.LastCheckpointScene = SceneManager.GetActiveScene().name;
            GameStateManager.Instance.LastCheckpointPos = position;
            GameStateManager.Instance.HasCheckpoint = true;
            GameStateManager.Instance.Save();
        }
    }
}
