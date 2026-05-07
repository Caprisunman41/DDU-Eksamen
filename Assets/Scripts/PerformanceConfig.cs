using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PerformanceConfig : MonoBehaviour
{
    [Tooltip("Target frame rate. -1 = uncapped (recommended for desktop)")]
    [SerializeField] private int targetFrameRate = -1;
    [Tooltip("0 = vSync off (lowest input lag), 1 = every refresh")]
    [SerializeField] private int vSyncCount = 0;

    private void Awake()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = 1f / 60f; // 60Hz physics for smoother input feel
        DontDestroyOnLoad(gameObject);
    }
}
