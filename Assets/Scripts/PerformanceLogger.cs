using UnityEngine;

public class PerformanceLogger : MonoBehaviour
{
    [SerializeField] private float logDuration = 5f;
    [SerializeField] private float logInterval = 1f;

    private float _elapsed;
    private float _intervalTimer;
    private int _frameCount;
    private float _accumulatedDelta;
    private float _accumulatedUnscaled;

    private void Update()
    {
        _elapsed += Time.unscaledDeltaTime;
        _intervalTimer += Time.unscaledDeltaTime;
        _frameCount++;
        _accumulatedDelta += Time.deltaTime;
        _accumulatedUnscaled += Time.unscaledDeltaTime;

        if (_intervalTimer >= logInterval)
        {
            float avgFps = _frameCount / _accumulatedUnscaled;
            float avgDelta = _accumulatedDelta / _frameCount * 1000f;
            float avgUnscaled = _accumulatedUnscaled / _frameCount * 1000f;
            Debug.Log($"[PerfLogger] frames={_frameCount} avgFPS={avgFps:F1} avgDeltaMs={avgDelta:F2} unscaledMs={avgUnscaled:F2} vSync={QualitySettings.vSyncCount} targetFR={Application.targetFrameRate}");
            _frameCount = 0;
            _accumulatedDelta = 0f;
            _accumulatedUnscaled = 0f;
            _intervalTimer = 0f;
        }

        if (_elapsed >= logDuration)
        {
            Debug.Log("[PerfLogger] Logging done, disabling.");
            enabled = false;
        }
    }
}
