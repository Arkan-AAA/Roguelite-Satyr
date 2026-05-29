using System;
using UnityEngine;

namespace Misc {
    public class FlashBlink : MonoBehaviour {
        [SerializeField] private float blinkDuration = 0.2f;
        [SerializeField] private Color blinkColor = Color.white;

        private SpriteRenderer _spriteRenderer;
        private Material _originalMaterial;
        private Material _blinkMaterial;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) {
                Debug.LogError($"SpriteRenderer not found on {name}");
                return;
            }

            _originalMaterial = _spriteRenderer.material;
        }

        private void Start() {
            var enemyAI = GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.OnFlashBlink += (s, e) => Blink();
            else {
                var player = GetComponentInParent<Player>();
                if (player != null)
                    player.OnFlashBlink += (s, e) => Blink();
                else
                    Debug.LogWarning($"No EnemyAI or Player found for FlashBlink on {name}");
            }
        }

        public void Blink() {
            if (_spriteRenderer == null) return;
            StopAllCoroutines();
            StartCoroutine(DoBlink());
        }

        private System.Collections.IEnumerator DoBlink() {
            // Создаём временный материал на основе оригинального
            if (_blinkMaterial == null) {
                _blinkMaterial = new Material(_originalMaterial);
                _blinkMaterial.color = blinkColor;
            }

            _spriteRenderer.material = _blinkMaterial;
            yield return new WaitForSeconds(blinkDuration);
            _spriteRenderer.material = _originalMaterial;
        }

        public void StopBlinking() {
            StopAllCoroutines();
            if (_spriteRenderer != null)
                _spriteRenderer.material = _originalMaterial;
        }
    }
}