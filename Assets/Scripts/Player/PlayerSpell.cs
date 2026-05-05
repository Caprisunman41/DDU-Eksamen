using UnityEngine;

public class PlayerSpell : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (InputManager.SpellWasPressed)
            TryShoot();
    }

    private void TryShoot()
    {
        Debug.Log("TryShoot kaldt");

        if (ManaManager.Instance == null) { Debug.LogError("ManaManager mangler i scenen"); return; }
        if (bulletPrefab == null) { Debug.LogError("Bullet Prefab ikke sat i Inspector"); return; }
        if (firePoint == null) { Debug.LogError("Fire Point ikke sat i Inspector"); return; }

        if (InventoryManager.Instance != null && !InventoryManager.Instance.HasSpell) return;
        if (!ManaManager.Instance.TryUseMana()) { Debug.Log("Ingen mana"); return; }

        Vector2 direction = _controller.IsFacingRight ? Vector2.right : Vector2.left;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));
        bullet.GetComponent<Bullet>().Init(direction, GetComponent<Collider2D>());
        Debug.Log("Bullet spawnet i retning: " + direction);
    }
}
