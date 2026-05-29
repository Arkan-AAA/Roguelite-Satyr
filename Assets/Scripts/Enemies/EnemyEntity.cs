using System;
using Other;
using ScriptableObjects;
using UnityEngine;

namespace Enemies {
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyEntity : MonoBehaviour {
        [SerializeField]
        private EnemySO _enemySO;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => _enemySO?.enemyHealth ?? 0;

        public event Action OnHit;
        public event Action OnDeath;
        public event Action<int, int> OnHealthChanged; // ДОБАВИТЬ

        private KnockBack _knockBack;

        public int GetMaxHealth() => MaxHealth;

        private void Start() {
            CurrentHealth = _enemySO.enemyHealth;
            _knockBack = GetComponent<KnockBack>();
        }

        public void TakeDamage(Transform damageSource, int damage) {
            if (CurrentHealth <= 0)
                return;

            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); // ДОБАВИТЬ ВЫЗОВ
            _knockBack?.GetKnockedBack(damageSource);

            if (CurrentHealth <= 0) {
                OnDeath?.Invoke();
            }
            else {
                OnHit?.Invoke();
            }
        }
    }
}