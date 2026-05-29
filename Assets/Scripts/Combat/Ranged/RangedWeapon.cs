using System;
using UnityEngine;

namespace Combat {
    public abstract class RangedWeapon : Weapon {
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected float shootDelay = 0.5f;

        protected float lastShootTime;
        public event EventHandler OnShoot;

        public override void Attack() {
            if (Time.time < lastShootTime + shootDelay) return;
            lastShootTime = Time.time;
            Shoot();
        }

        protected virtual void Shoot() {
            if (projectilePrefab == null) return;

            GameObject projectile = Instantiate(projectilePrefab, GetSpawnPoint(), Quaternion.identity);
            Vector2 direction = GetDirection();
            if (projectile.TryGetComponent<Rigidbody2D>(out var rb)) {
                rb.linearVelocity = direction * projectileSpeed;
            }
            OnShoot?.Invoke(this, EventArgs.Empty);
        }

        protected virtual Vector2 GetSpawnPoint() {
            // Можно вернуть позицию самого оружия или смещение
            return transform.position;
        }

        protected virtual Vector2 GetDirection() {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (mousePos - transform.position).normalized;
        }
    }
}