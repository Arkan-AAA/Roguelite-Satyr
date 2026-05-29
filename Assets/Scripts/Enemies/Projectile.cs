using UnityEngine;

public class Projectile : MonoBehaviour {
    public int damage = 10;
    public float lifetime = 3f;

    private void Start() {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Попадание в игрока
        if (other.TryGetComponent<Player>(out var player)) {
            player.TakeDamage(transform, damage);
            Destroy(gameObject);
        }
        // Попадание в стену (опционально)
        else if (other.CompareTag("Wall")) {
            Destroy(gameObject);
        }
    }
}