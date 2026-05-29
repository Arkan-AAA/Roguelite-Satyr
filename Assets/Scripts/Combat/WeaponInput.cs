using UnityEngine;

public class WeaponInput : MonoBehaviour {
    private WeaponInventory inventory;

    private void Start() {
        inventory = GetComponent<WeaponInventory>();
    }

    private void OnEnable() {
        // Атака
        GameInput.Instance.OnPlayerAttack += (sender, e) => ActiveWeapon.Instance?.Attack();
        GameInput.Instance.OnPlayerAttackHeld += () => ActiveWeapon.Instance?.AttackHeld();
        GameInput.Instance.OnPlayerAttackReleased += () => ActiveWeapon.Instance?.AttackReleased();

        // Смена оружия
        GameInput.Instance.OnNextWeapon += () => inventory?.NextWeapon();
        GameInput.Instance.OnPreviousWeapon += () => inventory?.PreviousWeapon();
        GameInput.Instance.OnWeaponSlot += (slot) => inventory?.EquipWeapon(slot);
    }

    private void OnDisable() {
        // Отписываемся, чтобы избежать проблем при перезагрузке сцены
        GameInput.Instance.OnPlayerAttack -= (sender, e) => ActiveWeapon.Instance?.Attack();
        GameInput.Instance.OnPlayerAttackHeld -= () => ActiveWeapon.Instance?.AttackHeld();
        GameInput.Instance.OnPlayerAttackReleased -= () => ActiveWeapon.Instance?.AttackReleased();
        GameInput.Instance.OnNextWeapon -= () => inventory?.NextWeapon();
        GameInput.Instance.OnPreviousWeapon -= () => inventory?.PreviousWeapon();
        GameInput.Instance.OnWeaponSlot -= (slot) => inventory?.EquipWeapon(slot);
    }
}