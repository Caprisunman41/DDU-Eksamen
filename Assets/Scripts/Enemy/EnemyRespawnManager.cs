using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawnManager : MonoBehaviour
{
    public static EnemyRespawnManager Instance { get; private set; }

    private struct EnemyState
    {
        public GameObject obj;
        public Vector3 position;
        public float maxHealth;
    }

    private List<EnemyState> _enemies = new List<EnemyState>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            _enemies.Add(new EnemyState
            {
                obj = enemy.gameObject,
                position = enemy.transform.position,
                maxHealth = enemy.maxHealth
            });
        }
    }

    public void RespawnAllEnemies()
    {
        foreach (EnemyState state in _enemies)
        {
            if (state.obj == null) continue;

            state.obj.transform.position = state.position;
            state.obj.SetActive(true);

            EnemyHealth health = state.obj.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.health = state.maxHealth;
                health.isDead = false;
            }

            EnemyMovement movement = state.obj.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.isChasing = false;
                movement.isKnockedBack = false;
                movement.patrolDestination = 0;
            }
        }
    }
}
