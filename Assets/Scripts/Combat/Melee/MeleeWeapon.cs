using System;
using Combat;
using Enemies;
using UnityEngine;

namespace Combat {
    public abstract class MeleeWeapon : Weapon {
        [SerializeField] protected int damageAmount = 10;
        [SerializeField] protected float attackDuration = 0.2f;

        protected PolygonCollider2D hitboxCollider;
        public event EventHandler OnWeaponSwing;

        protected virtual void Awake() {
            hitboxCollider = GetComponent<PolygonCollider2D>();
            if (hitboxCollider != null)
                hitboxCollider.enabled = false;
        }

        public override void Attack() {
            if (hitboxCollider == null) return;

            hitboxCollider.enabled = true;
            OnWeaponSwing?.Invoke(this, EventArgs.Empty);

            // Автоматически выключить коллайдер через время
            Invoke(nameof(DisableCollider), attackDuration);
        }

        private void DisableCollider() {
            if (hitboxCollider != null)
                hitboxCollider.enabled = false;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent(out EnemyEntity enemy)) {
                enemy.TakeDamage(transform, damageAmount);
            }
        }
    }
}