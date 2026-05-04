using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float _spawnOffsetY = 1.5f;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameInterval = 0.08f;

    private SpriteRenderer _sr;
    private bool _activated;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && frames.Length > 0)
            _sr.sprite = frames[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_activated) return;

        _activated = true;
        CheckpointManager.Instance.SetCheckpoint(transform.position + Vector3.up * _spawnOffsetY, this);
        StartCoroutine(PlayActivation());
    }

    public void Deactivate()
    {
        StopAllCoroutines();
        _activated = false;
        if (_sr != null && frames.Length > 0)
            _sr.sprite = frames[0];
    }

    private IEnumerator PlayActivation()
    {
        foreach (Sprite frame in frames)
        {
            _sr.sprite = frame;
            yield return new WaitForSeconds(frameInterval);
        }

        _sr.sprite = frames[frames.Length - 1];
    }
}
